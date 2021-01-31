using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DevAdventCalendarCompetition.Extensions;
using DevAdventCalendarCompetition.Models.Manage;
using DevAdventCalendarCompetition.Resources;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace DevAdventCalendarCompetition.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private const string RecoveryCodesKey = nameof(RecoveryCodesKey);

        private readonly IManageService _manageService;
        private readonly IAccountService _accountService;
        private readonly ILogger _logger;
        private readonly INotificationService _emailNotificationService;
        private readonly IStatisticsService _statisticsService;
        private readonly IGoogleCalendarService _googleCalendarService;

        public ManageController(
            IManageService manageService,
            IAccountService accountService,
            ILogger<ManageController> logger,
            INotificationService emailNotificationService,
            IGoogleCalendarService googleCalendarService,
            IStatisticsService statisticsService)
        {
            this._manageService = manageService ?? throw new ArgumentNullException(nameof(manageService));
            this._accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._emailNotificationService = emailNotificationService ?? throw new ArgumentNullException(nameof(emailNotificationService));
            this._googleCalendarService = googleCalendarService ?? throw new ArgumentNullException(nameof(googleCalendarService));
            this._statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var model = new IndexViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                EmailNotificationsEnabled = user.EmailNotificationsEnabled,
                PushNotificationsEnabled = user.PushNotificationsEnabled,
                StatusMessage = this.StatusMessage
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var email = user.Email;
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.IsEmailConfirmed = user.EmailConfirmed;

            var shouldSendVerificationEmail = false;
            if (model.Email != email)
            {
                var setEmailResult = await this._manageService.SetEmailAsync(user, model.Email).ConfigureAwait(false);
                if (!setEmailResult.Succeeded)
                {
                    this.AddErrors(setEmailResult, "Email");
                    return this.View(model);
                }

                shouldSendVerificationEmail = true;
                model.EmailNotificationsEnabled = false;
            }

            var userName = user.UserName;
            if (model.Username != userName)
            {
                var setUserNameResult = await this._manageService.SetUserNameAsync(user, model.Username).ConfigureAwait(false);
                if (!setUserNameResult.Succeeded)
                {
                    this.AddErrors(setUserNameResult, "Username");
                    return this.View(model);
                }
            }

            if (model.EmailNotificationsEnabled != user.EmailNotificationsEnabled)
            {
                user.EmailNotificationsEnabled = model.EmailNotificationsEnabled;
                var updateEmailNotificationPreferencesSucceeded =
                    await this._emailNotificationService
                        .SetSubscriptionPreferenceAsync(user.Email, user.EmailNotificationsEnabled && user.EmailConfirmed)
                        .ConfigureAwait(false)
                    && (await this._manageService.UpdateUserAsync(user).ConfigureAwait(false)).Succeeded;

                if (updateEmailNotificationPreferencesSucceeded == false)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.ErrorDuringEmailNotificationsPreferenceChange, this._accountService.GetUserId(this.User)));
                }
            }

            if (model.PushNotificationsEnabled != user.PushNotificationsEnabled)
            {
                user.PushNotificationsEnabled = model.PushNotificationsEnabled;
                var updatePushNotificationPreferencesSucceeded = (await this._manageService.UpdateUserAsync(user).ConfigureAwait(false)).Succeeded;

                if (updatePushNotificationPreferencesSucceeded == false)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.ErrorDuringPushNotificationsPreferenceChange, this._accountService.GetUserId(this.User)));
                }
            }

            if (model.EmailNotificationsEnabled != user.EmailNotificationsEnabled)
            {
                user.EmailNotificationsEnabled = model.EmailNotificationsEnabled;
                var updateEmailNotificationPreferencesSucceeded =
                    await this._emailNotificationService
                        .SetSubscriptionPreferenceAsync(user.Email, user.EmailNotificationsEnabled && user.EmailConfirmed)
                        .ConfigureAwait(false)
                    && (await this._manageService.UpdateUserAsync(user).ConfigureAwait(false)).Succeeded;

                if (updateEmailNotificationPreferencesSucceeded == false)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.ErrorDuringEmailNotificationsPreferenceChange, this._accountService.GetUserId(this.User)));
                }
            }

            if (model.PushNotificationsEnabled != user.PushNotificationsEnabled)
            {
                user.PushNotificationsEnabled = model.PushNotificationsEnabled;
                var updatePushNotificationPreferencesSucceeded = (await this._manageService.UpdateUserAsync(user).ConfigureAwait(false)).Succeeded;

                if (updatePushNotificationPreferencesSucceeded == false)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.ErrorDuringPushNotificationsPreferenceChange, this._accountService.GetUserId(this.User)));
                }
            }

            if (shouldSendVerificationEmail)
            {
                return await this.SendVerificationEmail(model).ConfigureAwait(false);
            }

            this.StatusMessage = "Profil został zaktualizowany";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(IndexViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var code = await this._accountService.GenerateEmailConfirmationTokenAsync(this.User).ConfigureAwait(false);
            var callbackUrl = this.Url.EmailConfirmationLink(user.Id, code, this.Request.Scheme);
            var email = user.Email;
            var isNewEmail = true;
            await this._accountService.SendEmailConfirmationAsync(email, new Uri(callbackUrl), isNewEmail).ConfigureAwait(false);

            this.StatusMessage = "E-mail weryfikacyjny został wysłany. Sprawdź swoją skrzynkę odbiorczą";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var hasPassword = await this._manageService.HasPasswordAsync(user).ConfigureAwait(false);
            if (!hasPassword)
            {
                return this.RedirectToAction(nameof(this.SetPassword));
            }

            var model = new ChangePasswordViewModel { StatusMessage = this.StatusMessage };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var changePasswordResult = await this._manageService.ChangePasswordAsync(user, model.OldPassword, model.NewPassword).ConfigureAwait(false);

            if (!changePasswordResult.Succeeded)
            {
                this.AddErrors(changePasswordResult);
                return this.View(model);
            }

            await this._accountService.SignInAsync(user).ConfigureAwait(false);
            this._logger.LogInformation(LoggingMessages.PasswordIsChangedSuccessfully);
            this.StatusMessage = "Twoje hasło zostało zmienione";

            return this.RedirectToAction(nameof(this.ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var hasPassword = await this._manageService.HasPasswordAsync(user).ConfigureAwait(false);

            if (hasPassword)
            {
                return this.RedirectToAction(nameof(this.ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = this.StatusMessage };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var addPasswordResult = await this._manageService.AddPasswordAsync(user, model.NewPassword).ConfigureAwait(false);
            if (!addPasswordResult.Succeeded)
            {
                this.AddErrors(addPasswordResult);
                return this.View(model);
            }

            await this._accountService.SignInAsync(user).ConfigureAwait(false);
            this.StatusMessage = "Your password has been set.";

            return this.RedirectToAction(nameof(this.SetPassword));
        }

        public IActionResult AuthorizeAccessToGoogleCalendar()
        {
            return this.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = this.Url.Action(nameof(this.AuthorizationCallback), "Manage")
                },
                "Calendar");
        }

        public async Task<IActionResult> AuthorizationCallback()
        {
            var response = await this._googleCalendarService.CreateNewCalendarWithEvents();
            if (response.Status == OperationalResultStatus.Success)
            {
                await this.AddInfoToUserAboutSuccessfulIntegration();
            }

            this.StatusMessage = MapStatusToMessage(response.Status);
            return this.RedirectToAction(nameof(this.GoogleCalendarIntegration));
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCalendarIntegration()
        {
            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var userHasPermissions = await this.CheckIfUserHasPermissions();
            var model = new GoogleCalendarViewModel
            {
                IsAlreadyIntegrated = user.IsIntegratedWithGoogleCalendar,
                HasPermissions = userHasPermissions,
                StatusMessage = this.StatusMessage
            };
            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DisplayStatistics()
        {
            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            var testStats = this._statisticsService.FillResultsWithTestStats(user.Id);
            List<DisplayStatisticsViewModel> viewTestStats = new List<DisplayStatisticsViewModel>();

            foreach (var stat in testStats)
            {
                viewTestStats.Add(new DisplayStatisticsViewModel()
                {
                    CorrectAnswerDate = (stat.CorrectAnswerDateTime == DateTime.MinValue) ? null : stat.CorrectAnswerDateTime,
                    WrongAnswerCount = stat.WrongAnswerCount,
                    WrongAnswers = stat.WrongAnswers,
                    TestNumber = stat.TestNumber
                });
            }

            return this.View(viewTestStats);
        }

        private static string MapStatusToMessage(OperationalResultStatus status) =>
            status switch
            {
                OperationalResultStatus.Success => ViewsMessages.GoogleCalendarSuccess,
                OperationalResultStatus.CalendarFailure => ViewsMessages.GoogleCalendarError,
                OperationalResultStatus.EventsFailure => ViewsMessages.GoogleEventsError,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };

        private async Task<bool> CheckIfUserHasPermissions()
        {
            return await this.HttpContext.GetTokenAsync("Calendar", "access_token") != null;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError("Result", error.Description);
            }
        }

        private void AddErrors(IdentityResult result, string key)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(key, error.Description);
            }
        }

        private async Task AddInfoToUserAboutSuccessfulIntegration()
        {
            var user = await this._manageService.GetUserAsync(this.User).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    ExceptionsMessages.UserWithIdNotFound, this._accountService.GetUserId(this.User)));
            }

            user.IsIntegratedWithGoogleCalendar = true;
            await this._manageService.UpdateUserAsync(user).ConfigureAwait(false);
        }
    }
}
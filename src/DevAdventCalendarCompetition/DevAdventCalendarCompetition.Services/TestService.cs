using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.Services.Extensions;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Models;
using Microsoft.Extensions.Configuration;

namespace DevAdventCalendarCompetition.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly IUserTestAnswersRepository _testAnswerRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly StringHasher _stringHasher;

        public TestService(
            ITestRepository testRepository,
            IUserTestAnswersRepository testAnswerRepository,
            IMapper mapper,
            StringHasher stringHasher,
            IConfiguration configuration)
        {
            this._testRepository = testRepository;
            this._testAnswerRepository = testAnswerRepository;
            this._mapper = mapper;
            this._configuration = configuration;
            this._stringHasher = stringHasher;
        }

        // w kazdej metodzie sprawdzić czy is advent true jesli nie  to exeptions
        public TestDto GetTestByNumber(int testNumber)
        {
            if (IsAdventExtensions.CheckIsAdvent(this._configuration) == false)
            {
#pragma warning disable CA2201 // Do not raise reserved exception types
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new Exception("Brak Adwentu");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore CA2201 // Do not raise reserved exception types
            }

            var test = this._testRepository.GetTestByNumber(testNumber);

            var testDto = this._mapper.Map<TestDto>(test);
            return testDto;
        }

        public void AddTestAnswer(int testId, string userId, DateTime testStartDate)
        {
            var currentTime = DateTime.Now;
            var answerTimeOffset = currentTime.Subtract(testStartDate);
            var maxAnswerTime = new TimeSpan(0, 23, 59, 59, 999);

            var testAnswer = new UserTestCorrectAnswer()
            {
                TestId = testId,
                UserId = userId,
                AnsweringTime = currentTime,
                AnsweringTimeOffset = answerTimeOffset > maxAnswerTime ? maxAnswerTime : answerTimeOffset
            };

            // TODO remove (for tests only)
            this._testAnswerRepository.AddCorrectAnswer(testAnswer);
        }

        public UserTestCorrectAnswerDto GetAnswerByTestId(int testId)
        {
            var testAnswer = this._testAnswerRepository.GetCorrectAnswerByTestId(testId);
            var testAnswerDto = this._mapper.Map<UserTestCorrectAnswerDto>(testAnswer);
            return testAnswerDto;
        }

        public bool HasUserAnsweredTest(string userId, int testId)
        {
            return this._testAnswerRepository.HasUserAnsweredTest(userId, testId);
        }

        public void AddTestWrongAnswer(string userId, int testId, string wrongAnswer, DateTime wrongAnswerDate)
        {
            var testWrongAnswer = new UserTestWrongAnswer()
            {
                UserId = userId,
                Time = wrongAnswerDate,
                Answer = wrongAnswer,
                TestId = testId
            };

            this._testAnswerRepository.AddWrongAnswer(testWrongAnswer);
        }

        public bool VerifyTestAnswer(string userAnswer, IEnumerable<string> correctAnswers)
        {
            return correctAnswers.Any(t => this._stringHasher.VerifyHash(userAnswer, t));
        }
    }
}
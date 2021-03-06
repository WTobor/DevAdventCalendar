using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.Services.Options;
using DevAdventCalendarCompetition.TestResultService.Tests.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DevAdventCalendarCompetition.TestResultService.Tests
{
    [Collection(nameof(TestResultServiceTestCollection))]
    public class TestResultServiceTest
    {
        private readonly TestResultServiceTestFixture fixture;
        private readonly TestResultRepository testResultRepository;

        public TestResultServiceTest(TestResultServiceTestFixture fixture)
        {
            this.fixture = fixture;
            testResultRepository = fixture.GetTestResultRepository();
        }

        //Test for everything (final results)
        [Fact]
        public void GetFinalCorrectRanking()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(),
                new AnsweringTimePlaceRule(), GetTestSettings());
            var expectedResult = fixture.GetExpectedFinalResultModel();

            //Act
            service.CalculateFinalResults();
            var results = testResultRepository.GetFinalResults();

            //Assert
            var expectedResultItems = expectedResult.Select(x => GetFinalResult(x)).ToList();
            var resultItems = results.Select(x => GetFinalResult(x)).ToList();
            Assert.Equal(expectedResultItems.Count, resultItems.Count);
            foreach (var item in expectedResultItems)
            {
                Assert.Contains(item, resultItems);
            }
        }
        
        //Test for everything (week 1 results)
        [Fact]
        public void GetWeek1CorrectRanking()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());
            var expectedResult = fixture.GetExpectedWeek1ResultModel();

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            var expectedResultItems = expectedResult.Select(x => GetWeek1Result(x)).ToList();
            var resultItems = results.Select(x => GetWeek1Result(x)).ToList();
            Assert.Equal(expectedResultItems.Count, resultItems.Count);
            foreach (var item in expectedResultItems)
            {
                Assert.Contains(item, resultItems);
            }
        }

        //Test for everything (week 2 results)
        [Fact]
        public void GetWeek2CorrectRanking()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());
            var expectedResult = fixture.GetExpectedWeek2ResultModel();

            //Act
            service.CalculateWeeklyResults(2);
            var results = testResultRepository.GetFinalResults();

            //Assert
            var expectedResultItems = expectedResult.Select(x => GetWeek2Result(x)).ToList();
            var resultItems = results.Select(x => GetWeek2Result(x)).ToList();
            Assert.Equal(expectedResultItems.Count, resultItems.Count);
            foreach (var item in expectedResultItems)
            {
                Assert.Contains(item, resultItems);
            }
        }

        [Fact]
        public void UsersWithNoCorrectAnswersHaveZeroPoints()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());
            var expectedResult = fixture.GetExpectedWeek1ResultModel();

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            var expectedResultItems = expectedResult.Where(x => x.Week1Points == 0).Select(x => x.UserId).ToList();
            var resultItems = results.Where(x => x.Week1Points == 0).Select(x => x.UserId).ToList();
            Assert.Equal(expectedResultItems.Count, resultItems.Count);
            foreach (var item in expectedResultItems)
            {
                Assert.Contains(item, resultItems);
            }
        }

        [Fact]
        public void UsersWithTheSameCorrectAnswersAndTheSameBonusHaveTheSamePoints()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());
            var expectedResult = fixture.GetExpectedWeek1ResultModel();

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                results.Where(x => x.UserId == UserModel.userA.Id).Select(x => x.Week1Points).FirstOrDefault(),
                results.Where(x => x.UserId == UserModel.userB.Id).Select(x => x.Week1Points).FirstOrDefault());

            Assert.Equal(
                expectedResult.Where(x => x.UserId == UserModel.userA.Id).Select(x => x.Week1Points).FirstOrDefault(),
                results.Where(x => x.UserId == UserModel.userA.Id).Select(x => x.Week1Points).FirstOrDefault());
        }

        [Fact]
        public void UsersWithAllCorrectAnswersAndNoWrongAnswersHaveMaximumPoints()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                results.Where(x => x.UserId == UserModel.userD.Id).Select(x => x.Week1Points).FirstOrDefault(),
                2 * (100+30));
            Assert.Equal(
                results.Where(x => x.UserId == UserModel.userC.Id).Select(x => x.Week1Points).FirstOrDefault(),
                2 * (100 + 30));
        }

        [Fact]
        public void UsersWithTheSamePointArePlacedByTimeOffset()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                results.Where(x => x.UserId == UserModel.userD.Id).Select(x => x.Week1Points).FirstOrDefault(),
                results.Where(x => x.UserId == UserModel.userC.Id).Select(x => x.Week1Points).FirstOrDefault());

            Assert.Equal(
                2,
                results.Where(x => x.UserId == UserModel.userC.Id).Select(x => x.Week1Place).FirstOrDefault());

            Assert.Equal(
                1,
                results.Where(x => x.UserId == UserModel.userD.Id).Select(x => x.Week1Place).FirstOrDefault());
        }

        [Fact]
        public void UserWithWrongAnswerButNoCorrectAnswerShouldHaveZeroPoints()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                0,
                results.FirstOrDefault(x => x.UserId == UserModel.userE.Id).Week1Points.Value);
        }

        [Fact]
        public void UserWithCorrectAnswerAfterRankingPerionShouldHaveZeroPoints()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());

            //Act
            service.CalculateWeeklyResults(1);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                0,
                results.FirstOrDefault(x => x.UserId == UserModel.userI.Id).Week1Points.Value);
        }

        [Fact]
        public void UserWhoAnsweredInSecondWeekToTheFirstWeekTestShouldHaveSecondWeekPointsOnly()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());

            //Act
            service.CalculateWeeklyResults(2);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                130,
                results.FirstOrDefault(x => x.UserId == UserModel.userI.Id).Week2Points.Value);
        }

        [Fact]
        public void UserWithTwoCorrectAnswersForOneTestInSecondWeekShouldHaveCalculatedResultsForJustFirstAnswer()
        {
            //Arrange
            var service = new TestResultService(testResultRepository, new CorrectAnswerPointsRule(), new BonusPointsRule(), new AnsweringTimePlaceRule(), GetTestSettings());

            //Act
            service.CalculateWeeklyResults(2);
            var results = testResultRepository.GetFinalResults();

            //Assert
            Assert.Equal(
                130,
                results.FirstOrDefault(x => x.UserId == UserModel.userI.Id).Week2Points.Value);
        }

        private object GetWeek1Result(Result result)
        {
            return new { UserdId = result.UserId, Week1Points = result.Week1Points, Week1Place = result.Week1Points != 0 ? result.Week1Place : int.MaxValue };
        }

        private object GetWeek2Result(Result result)
        {
            return new { UserdId = result.UserId, Week2Points = result.Week2Points, Week2Place = result.Week2Points != 0 ? result.Week2Place : int.MaxValue };
        }

        private object GetFinalResult(Result result)
        {
            return new { UserdId = result.UserId, FinalPoints = result.FinalPoints, FinalPlace = result.FinalPoints != 0 ? result.FinalPlace : int.MaxValue };
        }

        private static TestSettings GetTestSettings()
        {
            return new TestSettings()
            {
                StartHour = new TimeSpan(13, 0, 0),
                EndHour = new TimeSpan(23, 59, 59)
            };
        }
    }
}

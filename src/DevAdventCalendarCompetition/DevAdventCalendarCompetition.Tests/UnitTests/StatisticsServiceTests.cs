using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AutoMapper;
using DevAdventCalendarCompetition.Repository;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.Services;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace DevAdventCalendarCompetition.Tests.UnitTests
{
    public class StatisticsServiceTests
    {
        private readonly Mock<IStatisticsRepository> _statisticsRepositoryMock;
        private readonly StatisticsService _statisticsService;

        private readonly List<StatisticsDto> _results = new List<StatisticsDto>
        {
            new StatisticsDto { CorrectAnswerDateTime = DateTime.MinValue.AddDays(1), WrongAnswerCount = 1, TestNumber = 1 },
            new StatisticsDto { CorrectAnswerDateTime = DateTime.MinValue.AddDays(2), WrongAnswerCount = 2, TestNumber = 2 },
            new StatisticsDto { CorrectAnswerDateTime = DateTime.MinValue.AddDays(3), WrongAnswerCount = 3, TestNumber = 3 }
        };

        public StatisticsServiceTests()
        {
            this._statisticsRepositoryMock = new Mock<IStatisticsRepository>();
            this._statisticsService = new StatisticsService(this._statisticsRepositoryMock.Object);
        }

        [Fact]
        public void GetStatisticsDtoList()
        {
            // ARRANGE
            List<string> wrAns = new List<string>()
            {
                "ans1",
                "ans2",
                "ans3"
            };
            string userId = "c611530e-bebd-41a9-ace2-951550edbfa0";

            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredCorrectMaxTestId(userId)).Returns(4);
            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredWrongMaxTestId(userId)).Returns(0);
            this._statisticsRepositoryMock.Setup(a => a.GetHighestTestNumber(4)).Returns(3);

            for (int i = 1; i < 4; i++)
            {
                this._statisticsRepositoryMock.Setup(a => a.GetUserTestCorrectAnswerDate(userId, i + 2)).Returns(DateTime.MinValue.AddDays(i));
                this._statisticsRepositoryMock.Setup(a => a.GetUserTestWrongAnswerCount(userId, i + 2)).Returns(i);
                this._statisticsRepositoryMock.Setup(a => a.GetTestIdFromTestNumber(i)).Returns(i + 2);
                this._statisticsRepositoryMock.Setup(a => a.GetUserTestWrongAnswerString(userId, i + 2)).Returns(wrAns);
            }

            // ACT
            var result = this._statisticsService.FillResultsWithTestStats(userId);

            // ASSERT
            Assert.Equal(result[0].TestNumber, this._results[0].TestNumber);
            Assert.Equal(result[1].CorrectAnswerDateTime, this._results[1].CorrectAnswerDateTime);
            Assert.Equal(result[2].WrongAnswerCount, this._results[2].WrongAnswerCount);
        }

        [Fact]
        public void GetHighestTestNumberForNoTests()
        {
            // ARRANGE
            string userId = "c611530e-bebd-41a9-ace2-951550edbfa0";
            int testNumber = 0;

            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredCorrectMaxTestId(userId)).Returns(0);
            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredWrongMaxTestId(userId)).Returns(0);
            this._statisticsRepositoryMock.Setup(a => a.GetHighestTestNumber(0)).Returns(0);

            // ACT
            var result = this._statisticsService.GetHighestTestNumber(userId);

            // ASSERT
            Assert.Equal(result, testNumber);
        }

        [Fact]
        public void GetHighestTestNumberForNoCorrectAnswers()
        {
            // ARRANGE
            string userId = "c611530e-bebd-41a9-ace2-951550edbfa0";
            int testNumber = 2;

            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredCorrectMaxTestId(userId)).Returns(0);
            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredWrongMaxTestId(userId)).Returns(4);
            this._statisticsRepositoryMock.Setup(a => a.GetHighestTestNumber(4)).Returns(2);

            // ACT
            var result = this._statisticsService.GetHighestTestNumber(userId);

            // ASSERT
            Assert.Equal(result, testNumber);
        }

        [Fact]
        public void GetHighestTestNumberForNoWrongAnswers()
        {
            // ARRANGE
            string userId = "c611530e-bebd-41a9-ace2-951550edbfa0";
            int testNuber = 2;

            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredCorrectMaxTestId(userId)).Returns(4);
            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredWrongMaxTestId(userId)).Returns(0);
            this._statisticsRepositoryMock.Setup(a => a.GetHighestTestNumber(4)).Returns(2);

            // ACT
            var result = this._statisticsService.GetHighestTestNumber(userId);

            // ASSERT
            Assert.Equal(result, testNuber);
        }

        [Fact]
        public void GetHighestTestNumberForSomeWrongAndCorrectAnswers()
        {
            // ARRANGE
            string userId = "c611530e-bebd-41a9-ace2-951550edbfa0";
            int testNuber = 2;

            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredCorrectMaxTestId(userId)).Returns(4);
            this._statisticsRepositoryMock.Setup(a => a.GetAnsweredWrongMaxTestId(userId)).Returns(3);
            this._statisticsRepositoryMock.Setup(a => a.GetHighestTestNumber(4)).Returns(2);
            this._statisticsRepositoryMock.Setup(a => a.GetHighestTestNumber(3)).Returns(1);

            // ACT
            var result = this._statisticsService.GetHighestTestNumber(userId);

            // ASSERT
            Assert.Equal(result, testNuber);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace DevAdventCalendarCompetition.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepository;

        public StatisticsService(IStatisticsRepository statisticsRepository)
        {
            this._statisticsRepository = statisticsRepository;
        }

        public List<StatisticsDto> FillResultsWithTestStats(string userId)
        {
            List<StatisticsDto> allCurrentStats = new List<StatisticsDto>();

            int currentTestId;
            int maxNuber = this.GetHighestTestNumber(userId);
            for (int i = 1; i <= maxNuber; i++)
            {
                currentTestId = this._statisticsRepository.GetTestIdFromTestNumber(i);

                List<string> wrAns = this._statisticsRepository.GetUserTestWrongAnswerString(userId, currentTestId);
                for (int j = 0; j < wrAns.Count - 1; j++)
                {
                    wrAns[j] += ", ";
                }

                allCurrentStats.Add(new StatisticsDto()
                {
                    CorrectAnswerDateTime = this._statisticsRepository.GetUserTestCorrectAnswerDate(userId, currentTestId),
                    WrongAnswerCount = this._statisticsRepository.GetUserTestWrongAnswerCount(userId, currentTestId),
                    WrongAnswers = wrAns,
                    TestNumber = i
                });
            }

            return allCurrentStats;
        }

        public int GetHighestTestNumber(string userId)
        {
            int correctMaxTestNumber = this._statisticsRepository.GetHighestTestNumber(
                this._statisticsRepository.GetAnsweredCorrectMaxTestId(userId));
            int wrongMaxTestNumber = this._statisticsRepository.GetHighestTestNumber(
                this._statisticsRepository.GetAnsweredWrongMaxTestId(userId));

            return correctMaxTestNumber >= wrongMaxTestNumber
                ? correctMaxTestNumber : wrongMaxTestNumber;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DevAdventCalendarCompetition.TestResultService
{
    public class TestResultRepository : ITestResultRepository
    {
        public Task GetAnsweringTimeSum(DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCorrectAnswersCount(DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            throw new NotImplementedException();
        }

        public void GetFinalResults()
        {
            throw new NotImplementedException();
        }

        public Task GetWeeklyResults(int WeekNumber)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetWrongAnswersCount(DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            throw new NotImplementedException();
        }

        public void SaveFinalResults()
        {
            throw new NotImplementedException();
        }

        public Task SaveUserFinalPlace(int userId, int Place)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserFinalScore(int userId, int Place)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserWeeklyPlace(int userId, int WeekNumber, int Place)
        {
            throw new NotImplementedException();
        }

        public Task SaveUserWeeklyScore(int userId, int WeekNumber, int Score)
        {
            throw new NotImplementedException();
        }
    }
}

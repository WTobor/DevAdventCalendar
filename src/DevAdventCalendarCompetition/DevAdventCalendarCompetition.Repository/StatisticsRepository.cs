using System;
using System.Collections.Generic;
using System.Linq;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Migrations;
using DevAdventCalendarCompetition.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace DevAdventCalendarCompetition.Repository
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public StatisticsRepository(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public int GetUserTestWrongAnswerCount(string userId, int testId)
        {
            return this._dbContext
                .UserTestWrongAnswers
                .Count(a => a.TestId == testId && a.UserId == userId);
        }

        public DateTime? GetUserTestCorrectAnswerDate(string userId, int testId)
        {
            return this._dbContext
                .UserTestCorrectAnswers
                .Where(a => a.TestId == testId && a.UserId == userId)
                .Select(a => a.AnsweringTime)
                .SingleOrDefault();
        }

        public int GetAnsweredCorrectMaxTestId(string userId)
        {
            return this._dbContext.UserTestCorrectAnswers.Where(a => a.UserId == userId).Any()
                ? this._dbContext
                .UserTestCorrectAnswers
                .Where(a => a.UserId == userId)
                .Max(a => a.TestId)
                : 0;
        }

        public int GetAnsweredWrongMaxTestId(string userId)
        {
            return this._dbContext.UserTestWrongAnswers.Where(a => a.UserId == userId).Any()
                ? this._dbContext
                .UserTestWrongAnswers
                .Where(a => a.UserId == userId)
                .Max(a => a.TestId)
                : 0;
        }

        public int GetHighestTestNumber(int testId)
        {
            return this._dbContext.Tests.Any()
                ? this._dbContext
                .Tests
                .Where(a => a.Id == testId)
                .Select(a => a.Number)
                .SingleOrDefault()
                : 0;
        }

        public int GetTestIdFromTestNumber(int testNumber)
        {
            return this._dbContext.Tests.Any()
                ? this._dbContext
                .Tests
                .Where(a => a.Number == testNumber)
                .Select(a => a.Id)
                .SingleOrDefault()
                : 0;
        }

        public List<string> GetUserTestWrongAnswerString(string userId, int testId)
        {
            return this._dbContext.Tests.Any()
                ? this._dbContext
                .UserTestWrongAnswers
                .Where(a => a.TestId == testId && a.UserId == userId)
                .Select(a => a.Answer)
                .ToList()
                : null;
        }
    }
}

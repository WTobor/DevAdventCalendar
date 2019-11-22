﻿using System;
using System.Collections.Generic;
using System.Linq;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.TestResultService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevAdventCalendarCompetition.TestResultService
{
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TestResultRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string[] GetUsersId()
        {
            return this._dbContext
                .Users
                .Select(u => u.Id)
                .ToArray();
        }

        public int GetAnsweringTimeSum(string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            return _dbContext
                .TestAnswer
                .Where(a => a.UserId == userId && a.AnsweringTime > dateFrom && a.AnsweringTime <= dateTo)
                .Sum(a => a.AnsweringTimeOffset.Seconds);
        }

        public int GetCorrectAnswersCount(string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            return _dbContext
                .TestAnswer
                .Where(a => a.AnsweringTime > dateFrom && a.AnsweringTime <= dateTo)
                .Count(a => a.UserId == userId);
        }

        public List<Result> GetFinalResults()
        {
            return _dbContext
                .Results
                .ToList();
        }

        public int[] GetWrongAnswersCountPerDay(string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {
            return _dbContext
                .TestWrongAnswer
                .Where(a => a.Time >= dateFrom && a.Time <= dateTo && a.UserId == userId)
                .GroupBy(a => a.Time)
                .Select(c => c.Count())
                .ToArray();
        }

        public void SaveUserFinalPlace(string userId, int place)
        {
            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            if (userResult != null)
            {
                userResult.FinalPlace = place;
                _dbContext.Entry(userResult).Property(p => p.FinalPlace).IsModified = true;
                _dbContext.Update(userResult);
            }
            else
            {
                _dbContext.Results.Add(new Result { UserId = userId, FinalPlace = place });
            }

            _dbContext.SaveChanges();
        }

        public void SaveUserFinalScore(string userId, int score)
        {
            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            if (userResult != null)
            {
                userResult.FinalPoints = score;
                _dbContext.Entry(userResult).Property(p => p.FinalPoints).IsModified = true;
                _dbContext.Update(userResult);
            }
            else
            {
                _dbContext.Results.Add(new Result { UserId = userId, FinalPoints = score });
            }

            _dbContext.SaveChanges();
        }

        public void SaveUserWeeklyPlace(string userId, int weekNumber, int place)
        {
            Console.WriteLine($"\n\nGoing to save place for user { userId } and week { weekNumber }...");

            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            try
            {
                if (userResult != null)
                {
                    var property = userResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Place"));

                    if (property != null)
                        property.SetValue(userResult, place);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Update(userResult);
                }
                else
                {
                    var newResult = new Result {UserId = userId};

                    var property = newResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Place"));

                    if (property != null)
                        property.SetValue(newResult, place);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Results.Add(newResult);
                }

                _dbContext.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine($"\n\nAn error occurred during saving place for user { userId } and week { weekNumber }: { e.Message }");
            }
        }

        public void SaveUserWeeklyScore(string userId, int weekNumber, int score)
        {
            Console.WriteLine($"\n\nGoing to save score for user { userId } and week { weekNumber }...");

            var userResult = _dbContext
                .Results
                .FirstOrDefault(r => r.UserId == userId);

            try
            {
                if (userResult != null)
                {
                    var property = userResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Points"));

                    if (property != null)
                        property.SetValue(userResult, score);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Update(userResult);
                }
                else
                {
                    var newResult = new Result {UserId = userId};

                    var property = newResult.GetType().GetProperties()
                        .FirstOrDefault(p => p.Name.Contains($"Week{weekNumber.ToString()}Points"));

                    if (property != null)
                        property.SetValue(newResult, score);
                    else
                        throw new ArgumentException($"Missing week {weekNumber} in model.");

                    _dbContext.Results.Add(newResult);
                }

                _dbContext.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine($"\n\nAn error occurred during saving score for user { userId } and week { weekNumber }: { e.Message }");
            }
        }
    }
}

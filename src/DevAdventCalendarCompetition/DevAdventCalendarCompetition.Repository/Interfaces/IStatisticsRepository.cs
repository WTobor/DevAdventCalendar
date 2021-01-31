using System;
using System.Collections.Generic;
using System.Text;

namespace DevAdventCalendarCompetition.Repository.Interfaces
{
    public interface IStatisticsRepository
    {
        public int GetUserTestWrongAnswerCount(string userId, int testId);

        public DateTime? GetUserTestCorrectAnswerDate(string userId, int testId);

        public int GetAnsweredCorrectMaxTestId(string userId);

        public int GetAnsweredWrongMaxTestId(string userId);

        public int GetHighestTestNumber(int testId);

        public int GetTestIdFromTestNumber(int testNumber);

        public List<string> GetUserTestWrongAnswerString(string userId, int testId);
    }
}

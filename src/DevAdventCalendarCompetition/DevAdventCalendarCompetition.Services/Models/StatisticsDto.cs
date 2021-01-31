using System;
using System.Collections.Generic;

namespace DevAdventCalendarCompetition.Services.Models
{
    public class StatisticsDto
    {
        public int WrongAnswerCount { get; set; }

        public DateTime? CorrectAnswerDateTime { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> WrongAnswers { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public int TestNumber { get; set; }
    }
}
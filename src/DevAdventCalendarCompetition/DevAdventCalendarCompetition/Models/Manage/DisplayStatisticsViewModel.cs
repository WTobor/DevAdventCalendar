using System;
using System.Collections.Generic;

namespace DevAdventCalendarCompetition.Models.Manage
{
    public class DisplayStatisticsViewModel
    {
        public int WrongAnswerCount { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> WrongAnswers { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public DateTime? CorrectAnswerDate { get; set; }

        public int TestNumber { get; set; }
    }
}
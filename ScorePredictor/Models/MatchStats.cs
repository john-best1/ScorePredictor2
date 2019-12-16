using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class MatchStats
    {
        public string position { get; set; }
        public string HomeOrAwayPosition { get; set; }
        public string name { get; set; }
        public int matchesPlayed { get; set; }
        public int won { get; set; }
        public int drawn { get; set; }
        public int lost { get; set; }
        public int homeOrAwayMatchesPlayed { get; set; }
        public int homeOrAwayWon { get; set; }
        public int homeOrAwayDrawn { get; set; }
        public int homeOrAwayLost { get; set; }
        public int goalsFor { get; set; }
        public int goalsAgainst { get; set; }
        public int goalDifference { get; set; }
        public int points { get; set; }
        public double goalsScoredPerGame { get; set; }
        public double goalsConcededPerGame { get; set; }
        public string WDL { get; set; }
        public string homeOrAwayWDL { get; set; }
        public int[] overallLastSix { get; set; }
        public int[] homeOrAwayLastSix { get; set; }
        public string overallFormString { get; set; }
        public string homeOrAwayFormString { get; set; }
        public double PredictionPoints { get; set; }
        public int overallLast6Scored { get; set; }
        public int overallLast6Conceded { get; set; }
        public int homeOrAwayLast6Scored { get; set; }
        public int homeOrAwayLast6Conceded { get; set; }
    }
}

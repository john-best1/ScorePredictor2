using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class MatchStats
    {
        public int position { get; set; }
        public string name { get; set; }
        public int matchesPlayed { get; set; }
        public int won { get; set; }
        public int drawn { get; set; }
        public int lost { get; set; }
        public int goalsFor { get; set; }
        public int goalsAgainst { get; set; }
        public int goalDifference { get; set; }
        public int points { get; set; }
        public double goalsScoredPerGame { get; set; }
        public double goalsConcededPerGame { get; set; }
        public string WDL { get; set; }
    }
}

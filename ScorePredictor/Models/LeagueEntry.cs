using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class LeagueEntry
    {
        //public int id { get; set; }
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
    }
}

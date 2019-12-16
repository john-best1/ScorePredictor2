using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class Match
    {
        public string MatchId { get; set; }

        public string HomeTeamName { get; set; }

        public int HomeTeamId { get; set; }

        public string AwayTeamName { get; set; }

        public int AwayTeamId { get; set; }

        public string Stadium { get; set; }

        public string UtcDate { get; set; }

        public string LeagueName { get; set; }

        public int LeagueId { get; set; }

        public string ImageUrl { get; set; }

        public MatchStats HomeStats { get; set; }

        public MatchStats AwayStats { get; set; }

        public int predictedResult { get; set; }

        public string predictionString { get; set; }

        public int[] predictedScore { get; set; }
    }
}

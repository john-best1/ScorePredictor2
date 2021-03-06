﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class Fixture
    {
        public string utcDate { get; set; }

        public string matchId { get; set; }

        public int homeTeamId { get; set; }

        public string homeTeamName { get; set; }

        public int awayTeamId { get; set; }

        public string awayTeamName { get; set; }

        public int leagueId { get; set; }

        public string leagueName { get; set; }

        public bool finished { get; set; }

        public int homeScore { get; set; } = -1;

        public int awayScore { get; set; } = -1;

        public int predictedHomeScore { get; set; } = -1;

        public int predictedAwayScore { get; set; } = -1;

        public bool postponed { get; set; }

        public bool strong { get; set; } = false;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class PredictionData
    {
        public int TotalPredictions { get; set; }

        public int TotalHomeWinsPredicted { get; set; }

        public int TotalAwayWinsPredicted { get; set; }

        public int TotalStrongHomeWinsPredicted { get; set; }

        public int TotalStrongAwayWinsPredicted { get; set; }

        public int TotalStrongWinsPredicted { get; set; }

        public int TotalDrawsPredicted { get; set; }

        public int TotalCorrect { get; set; }

        public int TotalCorrectScores { get; set; }

        public int TotalCorrectDraws { get; set; }

        public int TotalCorrectStrongWins { get; set; }

        public int TotalCorrectHomeWins { get; set; }

        public int TotalCorrectStrongHomeWins { get; set; }

        public int TotalCorrectAwayWins { get; set; }

        public int TotalCorrectStrongAwayWins { get; set; }
    }
}

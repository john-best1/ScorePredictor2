using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class PredictionStats
    {
        public int totalPredictions { get; set; }

        public int totalCorrect { get; set; }

        public int totalCorrectScore { get; set; }

        public double correctPercentage { get; set; }

        public double correctScorePercentage { get; set; }
    }
}

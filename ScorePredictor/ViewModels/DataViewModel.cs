using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.ViewModels
{
    public class DataViewModel
    {
        public DataViewModel(PredictionData data)
        {
            TotalPredictions = data.TotalPredictions;
            TotalHomeWinsPredicted = data.TotalHomeWinsPredicted;
            TotalAwayWinsPredicted = data.TotalAwayWinsPredicted;
            TotalStrongHomeWinsPredicted = data.TotalStrongHomeWinsPredicted;
            TotalStrongAwayWinsPredicted = data.TotalStrongAwayWinsPredicted;
            TotalStrongWinsPredicted = data.TotalStrongWinsPredicted;
            TotalDrawsPredicted = data.TotalDrawsPredicted;
            TotalCorrect = data.TotalCorrect;
            TotalCorrectScores = data.TotalCorrectScores;
            TotalCorrectDraws = data.TotalCorrectDraws;
            TotalCorrectStrongWins = data.TotalCorrectStrongWins;
            TotalCorrectHomeWins = data.TotalCorrectHomeWins;
            TotalCorrectStrongHomeWins = data.TotalCorrectStrongHomeWins;
            TotalCorrectAwayWins = data.TotalCorrectAwayWins;
            TotalCorrectStrongAwayWins = data.TotalCorrectStrongAwayWins;
            SetPercentages();
        }

        public decimal CorrectResultPercentage { get; set; }

        public decimal CorrectScorePercentage { get; set; }

        public decimal CorrectHomeWinPercentage { get; set; }

        public decimal CorrectAwayWinPercentage { get; set; }

        public decimal CorrectDrawPercentage { get; set; }

        public decimal CorrectStrongWinPercentage { get; set; }

        public decimal CorrectStrongHomeWinPercentage { get; set; }

        public decimal CorrectStrongAwayWinPercentage { get; set; }

        public decimal HomeWinPredictedPercentage { get; set; }

        public decimal AwayWinPredictedPercentage { get; set; }

        public decimal DrawPredictedPercentage { get; set; }

        public decimal StrongWinPredictedPercentage { get; set; }

        public decimal StrongHomeWinPredictedPercentage { get; set; }

        public decimal StrongAwayWinPredictedPercentage { get; set; }

        public int TotalPredictions { get;  }

        public int TotalHomeWinsPredicted { get;  }

        public int TotalAwayWinsPredicted { get;  }

        public int TotalStrongHomeWinsPredicted { get;  }

        public int TotalStrongAwayWinsPredicted { get;  }

        public int TotalStrongWinsPredicted { get;  }

        public int TotalDrawsPredicted { get;  }

        public int TotalCorrect { get;  }

        public int TotalCorrectScores { get;  }

        public int TotalCorrectDraws { get;  }

        public int TotalCorrectStrongWins { get;  }

        public int TotalCorrectHomeWins { get;  }

        public int TotalCorrectStrongHomeWins { get;  }

        public int TotalCorrectAwayWins { get; }

        public int TotalCorrectStrongAwayWins { get;  }


        private void SetPercentages()
        {
            CorrectResultPercentage = decimal.Round(TotalCorrect / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            CorrectScorePercentage = decimal.Round(TotalCorrectScores / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            CorrectHomeWinPercentage = decimal.Round(TotalCorrectHomeWins / (decimal)TotalHomeWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectAwayWinPercentage = decimal.Round(TotalCorrectAwayWins / (decimal)TotalAwayWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectDrawPercentage = decimal.Round(TotalCorrectDraws / (decimal)TotalDrawsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectStrongWinPercentage = decimal.Round(TotalCorrectStrongWins / (decimal)TotalStrongWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectStrongHomeWinPercentage = decimal.Round(TotalCorrectStrongHomeWins / (decimal)TotalStrongHomeWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectStrongAwayWinPercentage = decimal.Round(TotalCorrectStrongAwayWins / (decimal)TotalStrongAwayWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            HomeWinPredictedPercentage = decimal.Round(TotalHomeWinsPredicted / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            AwayWinPredictedPercentage = decimal.Round(TotalAwayWinsPredicted / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            DrawPredictedPercentage = decimal.Round(TotalDrawsPredicted / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            StrongWinPredictedPercentage = decimal.Round(TotalStrongWinsPredicted / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            StrongHomeWinPredictedPercentage = decimal.Round(TotalStrongHomeWinsPredicted / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            StrongAwayWinPredictedPercentage = decimal.Round(TotalStrongAwayWinsPredicted / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
        }
    }
}
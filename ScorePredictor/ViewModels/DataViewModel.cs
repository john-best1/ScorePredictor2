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


        public void SetPercentages()
        {
            CorrectResultPercentage = decimal.Round(TotalCorrect / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            CorrectScorePercentage = decimal.Round(TotalCorrectScores / (decimal)TotalPredictions * 100, 1, MidpointRounding.AwayFromZero);
            CorrectHomeWinPercentage = decimal.Round(TotalCorrectHomeWins / (decimal)TotalHomeWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectAwayWinPercentage = decimal.Round(TotalCorrectAwayWins / (decimal)TotalAwayWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectDrawPercentage = decimal.Round(TotalCorrectDraws / (decimal)TotalDrawsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectStrongWinPercentage = decimal.Round(TotalCorrectStrongWins / (decimal)TotalStrongWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectStrongHomeWinPercentage = decimal.Round(TotalCorrectStrongHomeWins / (decimal)TotalStrongHomeWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
            CorrectStrongAwayWinPercentage = decimal.Round(TotalCorrectStrongAwayWins / (decimal)TotalStrongAwayWinsPredicted * 100, 1, MidpointRounding.AwayFromZero);
        }
    }
}

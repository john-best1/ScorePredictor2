using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.ViewModels
{
    public class FixturesViewModel
    {
        public FixtureList[] fixtureLists { get; set; }

        public DateTime date { get; set; }

        public PredictionStats predictionStats { get; set; }
    }
}

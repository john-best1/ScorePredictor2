using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.ViewModels
{
    public class MatchViewModel
    {
        public MatchViewModel(Match match)
        {
            this.match = match;
        }

        public Match match { get; set; }
    }
}

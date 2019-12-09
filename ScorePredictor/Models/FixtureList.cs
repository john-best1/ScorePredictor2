using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class FixtureList
    {
        public List<Fixture> fixtures { get; set; }
        public string leagueName { get; set; }
        public string utcDate { get; set; }

    }

}

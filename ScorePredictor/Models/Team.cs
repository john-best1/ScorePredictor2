using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.Models
{
    public class Team
    {
        public Team()
        {

        }
        public Team(string lName, string sName, int id, int lId)
        {
            LongName = lName;
            ShortName = sName;
            TeamId = id;
            LeagueId = lId;
        }

        public String LongName { get; set; }
        public String ShortName { get; set; }
        public int TeamId { get; set;}
        public int LeagueId { get;set; }
    }
}

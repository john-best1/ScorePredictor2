using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.ViewModels
{

    public class LeaguesViewModel
    {

        string[] leagueOptions = { "Overall", "Home", "Away" };
        public LeaguesViewModel(int leagueCode, int leagueTypeCode)
        {
            leagueType = leagueOptions[leagueTypeCode];
            this.leagueCode = leagueCode;
            this.leagueTypeCode = leagueTypeCode;
        }

        public int leagueTypeCode { get; set; }

        public int leagueCode { get; set; }

        public List<LeagueEntry> leagues { get; set; }

        public string shortLeagueTitle { get; set; }

        public string longLeagueTitle { get; set; }

        public string leagueType { get; set; }


    }
}

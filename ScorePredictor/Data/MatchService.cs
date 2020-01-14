using Newtonsoft.Json.Linq;
using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Data
{
    public class MatchService
    {
        Match match = new Match();
        LeagueService leagueService = new LeagueService();
        MatchDatabase database = new MatchDatabase();
        HttpClient client = new HttpClient();
        int season = 2019;   //change this every season
        string flagUrl;


        public async Task<Match> getMatch(string matchId, string flagUrl)
        {
            this.flagUrl = flagUrl;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";
            match.MatchId = matchId;
            match.ImageUrl = flagUrl;

            return await database.getMatch(match);
        }

    }
}

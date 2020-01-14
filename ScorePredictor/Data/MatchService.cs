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


        private void recordPredictionStatAdded(SqlConnectionStringBuilder builder) {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE Match set PredictionResultRecorded = 1 WHERE MatchId = " + match.MatchId + "; ", connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private void incrementPredictions(SqlConnectionStringBuilder builder)
        {
            // 1,2 =home win, 3 = draw, 4,5 = away win
            String query = "UPDATE PredictionTally SET TotalPredictions = TotalPredictions + 1";
            if(match.predictedScore[0] == match.homeGoals && match.predictedScore[1] == match.awayGoals)
            {
                query += ", TotalCorrect = TotalCorrect + 1, TotalCorrectScores = TotalCorrectScores + 1 ";
            }
            else if ((match.homeGoals == match.awayGoals && match.predictedResult == 3)||
                ((match.homeGoals > match.awayGoals) && (match.predictedResult == 1 || ((match.homeGoals > match.awayGoals) && match.predictedResult == 2)))||
                ((match.homeGoals < match.awayGoals) && (match.predictedResult == 4 || ((match.homeGoals < match.awayGoals) && match.predictedResult == 5))))
            {
                query += ", TotalCorrect = TotalCorrect + 1 ";
            }
            using(SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using(SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }

            match.predictionResultRecorded = true;
            recordPredictionStatAdded(builder);
        }
    }
}

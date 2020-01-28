using ScorePredictor.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ScorePredictor.Data
{
    public class DataService
    {
        SqlConnectionStringBuilder builder;
        HttpClient client = new HttpClient();

        public DataService()
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
        }


        public PredictionData getPredictionData()
        {
            PredictionData predictionData = new PredictionData();
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("SELECT * from PredictionTally as data ", connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            reader.Read();
                            predictionData.TotalPredictions = int.Parse(reader["TotalPredictions"].ToString());
                            predictionData.TotalHomeWinsPredicted = int.Parse(reader["TotalPredictedHomeWins"].ToString());
                            predictionData.TotalAwayWinsPredicted = int.Parse(reader["TotalPredictedAwayWins"].ToString());
                            predictionData.TotalStrongHomeWinsPredicted = int.Parse(reader["TotalPredictedStrongHomeWins"].ToString());
                            predictionData.TotalStrongAwayWinsPredicted = int.Parse(reader["TotalPredictedStrongAwayWins"].ToString());
                            predictionData.TotalStrongWinsPredicted = int.Parse(reader["TotalPredictedStrongWins"].ToString());
                            predictionData.TotalDrawsPredicted = int.Parse(reader["TotalPredictedDraws"].ToString());
                            predictionData.TotalCorrect = int.Parse(reader["TotalCorrect"].ToString());
                            predictionData.TotalCorrectScores = int.Parse(reader["TotalCorrectScores"].ToString());
                            predictionData.TotalCorrectDraws = int.Parse(reader["CorrectDraws"].ToString());
                            predictionData.TotalCorrectStrongWins = int.Parse(reader["TotalOverallCorrectStrongWins"].ToString());
                            predictionData.TotalCorrectHomeWins = int.Parse(reader["TotalOverallCorrectHomeWins"].ToString());
                            predictionData.TotalCorrectStrongHomeWins = int.Parse(reader["TotalCorrectStrongHomeWins"].ToString());
                            predictionData.TotalCorrectAwayWins = int.Parse(reader["TotalOverallCorrectAwayWins"].ToString());
                            predictionData.TotalCorrectStrongAwayWins = int.Parse(reader["TotalCorrectStrongAwayWins"].ToString());
                            reader.Close();
                        }
                        connection.Close();
                    }
                }
                return predictionData;

            }catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}

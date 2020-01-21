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
    public class FixtureService
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

        HttpClient client = new HttpClient();
        string dateString;
        string competitionPriorityOrder = "?competitions=2021,2016,2030,2014,2077,2002,2004,2019,2121,2015,2142,2084,2003,2017,2009,2145,2137,2013,2008,2024,2119";

        public FixtureService()
        { 
            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");            
        }

        public async Task<IEnumerable<FixtureList>> getDaysFixtures(DateTime? date)
        {
            dateString = getDateString(date);
            if (daysFixturesInDatabase(getDatabaseDateString(dateString)))
            {
                // gefixturesfromdatabase
                return getFixturesFromDatabase();
            }
            else
            {
                //getFixturesFromAPI
                return await getFixturesFromApi(date);
            }
        }

        private IEnumerable<FixtureList> getFixturesFromDatabase()
        {
            List<FixtureList> fixtureLists = new List<FixtureList>();
            List<int> leagueIds = new List<int>();
            List<Fixture> allFixtures = new List<Fixture>();
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = new SqlCommand("SELECT * from Fixture as fixture " +
                        "Where fixture.FixtureListId like '%" + getDatabaseDateString(dateString) + "%'", connection))
                {
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Fixture fixture = new Fixture();
                            fixture.homeTeamName = reader["HomeTeamName"].ToString();
                            fixture.awayTeamName = reader["AwayTeamName"].ToString();
                            fixture.matchId = reader["MatchId"].ToString();
                            fixture.homeTeamId = int.Parse(reader["HomeTeamId"].ToString());
                            fixture.awayTeamId = int.Parse(reader["AwayTeamId"].ToString());
                            fixture.leagueName = reader["LeagueName"].ToString();
                            fixture.leagueId = int.Parse(reader["LeagueId"].ToString());
                            fixture.finished = (bool)reader["Finished"];
                            fixture.postponed = (bool)reader["Postponed"];
                            fixture.homeScore = (int)reader["HomeScore"];
                            fixture.awayScore = (int)reader["AwayScore"];
                            fixture.predictedHomeScore = (int)reader["PredictedHomeScore"];
                            fixture.predictedAwayScore = (int)reader["PredictedAwayScore"];
                            fixture.utcDate = reader["UtcDate"].ToString();
                            allFixtures.Add(fixture);
                        }
                        reader.Close();
                    }
                }
                using (SqlCommand sqlCommand = new SqlCommand("SELECT * from FixtureList as fixtureList " +
                        "Where fixturelist.FixtureListId like '%" + getDatabaseDateString(dateString) + "%'", connection))
                {
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FixtureList list = new FixtureList();
                            list.leagueName = reader["LeagueName"].ToString();
                            list.utcDate = reader["UtcDate"].ToString();
                            list.imageUrl = reader["ImageUrl"].ToString();
                            list.leagueId = int.Parse(reader["LeagueId"].ToString());
                            list.fixtures = new List<Fixture>();
                            foreach(Fixture fixture in allFixtures)
                            {
                                if(fixture.leagueId == list.leagueId)
                                {
                                    list.fixtures.Add(fixture);
                                }
                            }
                            fixtureLists.Add(list);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
                return reorderFixtureLists(fixtureLists);
            }
        }
        private async Task<IEnumerable<FixtureList>> getFixturesFromApi(DateTime? date)
        {
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches" + competitionPriorityOrder + "&" + dateString))
            {
                if (response.IsSuccessStatusCode)
                {
                    List<FixtureList> fixtureLists = new List<FixtureList>();
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray allMatches = (JArray)jsonObject["matches"];

                    for (int i = 0; i < allMatches.Count; i++)
                    {
                        string competitionId = allMatches[i]["competition"]["id"].ToString();
                        if (allMatches[i]["status"].ToString() != "CANCELLED")
                        {
                            Fixture fixture = new Fixture
                            {
                                matchId = allMatches[i]["id"].ToString(),
                                homeTeamId = int.Parse(allMatches[i]["homeTeam"]["id"].ToString()),
                                homeTeamName = allMatches[i]["homeTeam"]["name"].ToString(),
                                awayTeamId = int.Parse(allMatches[i]["awayTeam"]["id"].ToString()),
                                awayTeamName = allMatches[i]["awayTeam"]["name"].ToString(),
                                leagueId = int.Parse(allMatches[i]["competition"]["id"].ToString()),
                                leagueName = allMatches[i]["competition"]["name"].ToString(),
                                utcDate = allMatches[i]["utcDate"].ToString()
                            };
                            if (date < DateTime.Today)
                            {
                                if (allMatches[i]["status"].ToString() == "FINISHED")
                                {
                                    fixture.finished = true;
                                    fixture.homeScore = int.Parse(allMatches[i]["score"]["fullTime"]["homeTeam"].ToString());
                                    fixture.awayScore = int.Parse(allMatches[i]["score"]["fullTime"]["awayTeam"].ToString());
                                }
                                else
                                {
                                    fixture.postponed = true;
                                }

                            }
                            if (fixture.leagueId == 2084)
                            {
                                fixture.leagueName = "SPL";
                            }
                            else if (fixture.leagueId == 2013)
                            {
                                fixture.leagueName = "Série_A";
                            }
                            bool competitionExists = false;
                            foreach (FixtureList list in fixtureLists)
                            {
                                if (list.leagueName == fixture.leagueName)
                                {
                                    list.fixtures.Add(fixture);
                                    competitionExists = true;
                                    break;
                                }
                            }
                            if (!competitionExists || !fixtureLists.Any())
                            {
                                FixtureList fixtureList = new FixtureList();
                                fixtureList.leagueName = fixture.leagueName;
                                fixtureList.utcDate = allMatches[i]["utcDate"].ToString();
                                fixtureList.fixtures = new List<Fixture>();
                                fixtureList.fixtures.Add(fixture);
                                fixtureList.leagueId = fixture.leagueId;
                                fixtureLists.Add(fixtureList);
                            }
                        }
                    }

                    addFixturesToDatabase(fixtureLists);
                    return reorderFixtureLists(fixtureLists);
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        private void addFixturesToDatabase(IEnumerable<FixtureList> lists)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                foreach (FixtureList list in lists)
                {
                    if (list != null)
                    {
                        addImageUrl(list);
                        using (SqlCommand cmd = new SqlCommand("insert into FixtureList values(" +
                                            "@fixturelistid,@leaguename,@utcdate,@imageurl,@leagueid);", connection))
                        {
                            cmd.Parameters.AddWithValue("@fixturelistid", getDatabaseDateString(dateString) + list.leagueId);
                            cmd.Parameters.AddWithValue("@leaguename", list.leagueName);
                            cmd.Parameters.AddWithValue("@utcdate", list.utcDate);
                            cmd.Parameters.AddWithValue("@imageurl", list.imageUrl);
                            cmd.Parameters.AddWithValue("@leagueid", list.leagueId);
                            cmd.ExecuteNonQuery();
                        }
                        foreach (Fixture fixture in list.fixtures)
                        {
                            using (SqlCommand cmd = new SqlCommand("insert into Fixture values(" +
                                "@fixturelistid,@matchid,@hometeamid,@hometeamname," +
                                "@awayteamid, @awayteamname, @leagueid, @leaguename," +
                                "@finished, @homescore, @awayscore, @postponed, @utcdate, @predictedhomegoals, @predictedawaygoals);", connection))
                            {
                                cmd.Parameters.AddWithValue("@fixturelistid", getDatabaseDateString(dateString) + list.leagueId);
                                cmd.Parameters.AddWithValue("@matchid", fixture.matchId);
                                cmd.Parameters.AddWithValue("@hometeamid", fixture.homeTeamId);
                                cmd.Parameters.AddWithValue("@hometeamname", fixture.homeTeamName);
                                cmd.Parameters.AddWithValue("@awayteamid", fixture.awayTeamId);
                                cmd.Parameters.AddWithValue("@awayteamname", fixture.awayTeamName);
                                cmd.Parameters.AddWithValue("@leagueid", fixture.leagueId);
                                cmd.Parameters.AddWithValue("@leaguename", fixture.leagueName);
                                cmd.Parameters.AddWithValue("@finished", fixture.finished);
                                cmd.Parameters.AddWithValue("@homescore", fixture.homeScore);
                                cmd.Parameters.AddWithValue("@awayscore", fixture.awayScore);
                                cmd.Parameters.AddWithValue("@postponed", fixture.postponed);
                                cmd.Parameters.AddWithValue("@utcdate", fixture.utcDate);
                                cmd.Parameters.AddWithValue("@predictedhomegoals", fixture.predictedHomeScore);
                                cmd.Parameters.AddWithValue("@predictedawaygoals", fixture.predictedAwayScore);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

        private void addImageUrl(FixtureList list)
        {
            int[] competitionIDs = { 2021, 2016, 2030, 2014, 2077, 2002, 2004, 2019, 2121, 2015, 2142, 2084, 2003, 2017, 2009, 2145, 2137, 2013, 2008, 2024, 2119 };
            string[] imageUrls = {
                "/images/english-flag-small.jpg" ,
                "/images/english-flag-small.jpg" ,
                "/images/english-flag-small.jpg" ,
                "/images/spanish-flag-small.jpg" ,
                "/images/spanish-flag-small.jpg" ,
                "/images/german-flag-small.jpg" ,
                "/images/german-flag-small.jpg" ,
                "/images/italian-flag-small.jpg" ,
                "/images/italian-flag-small.jpg" ,
                "/images/french-flag-small.jpg" ,
                "/images/french-flag-small.jpg" ,
                "/images/scotland-flag-large.jpg" ,
                "/images/dutch-flag-small.jpg" ,
                "/images/portuguese-flag-small.jpg" ,
                "/images/belgian-flag-small.jpg" ,
                "/images/american-flag-small.jpg" ,
                "/images/russian-flag-small.jpg" ,
                "/images/brazilian-flag-small.jpg" ,
                "/images/australian-flag-small.jpg" ,
                "/images/argentinian-flag-small.jpg" ,
                "/images/japanese-flag-small.jpg"
            };
            list.imageUrl = imageUrls[Array.IndexOf(competitionIDs, list.leagueId)];
        }

        private bool daysFixturesInDatabase(string dateString)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) from FixtureList as fixturelist " +
                    "Where fixturelist.FixtureListId like '%" + dateString + "%'", connection))
                {
                    connection.Open();
                    int userCount = (int)sqlCommand.ExecuteScalar();
                    connection.Close();
                    if (userCount > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string getDatabaseDateString(string dateString)
        {
            return dateString.Substring(9, 4) + dateString.Substring(14, 2) + dateString.Substring(17, 2);
        }

        public PredictionStats getPredictionStats(PredictionStats predictionStats)
        {
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("Select * From PredictionTally; ", connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            predictionStats.totalPredictions = (int)reader["TotalPredictions"];
                            predictionStats.totalCorrect = (int)reader["TotalCorrect"];
                            predictionStats.totalCorrectScore = (int)reader["TotalCorrectScores"];
                            reader.Close();
                            connection.Close();
                        }
                    }
                }
            }

            if(predictionStats.totalCorrect > 0)
            {
                predictionStats.correctPercentage = Math.Round((double)predictionStats.totalCorrect / (double)predictionStats.totalPredictions * 100, 2);
            }
            else
            {
                predictionStats.correctPercentage = 0;
            }
            if(predictionStats.totalCorrectScore > 0)
            {
                predictionStats.correctScorePercentage = Math.Round((double)predictionStats.totalCorrectScore / (double)predictionStats.totalPredictions * 100,2);
            }
            else
            {
                predictionStats.correctScorePercentage = 0;
            }
            
            return predictionStats;
        }

        private string getDateString(DateTime? date)
        {
            string dateString = date.ToString();

            string day;
            string month;
            string year;
            if (dateString[1] == '/') //month less than 10
            {
                month = "0" + dateString.Substring(0, 1);
                if(dateString[3] == '/') //day < 10
                {
                    day = "0" + dateString[2];
                    year = dateString.Substring(4, 4);
                }
                else // day>=10
                {
                    day = dateString.Substring(2, 2);
                    year = dateString.Substring(5, 4);
                }
            }
            else //month >=10
            {
                month = dateString.Substring(0, 2);
                if (dateString[4] == '/') //day < 10
                {
                    day = "0" + dateString[3];
                    year = dateString.Substring(5, 4);
                }
                else // day >= 10
                {
                    day = dateString.Substring(3, 2);
                    year = dateString.Substring(6, 4);
                }
            }

            dateString = "dateFrom=" + year + "-" + month + "-" + day + "&dateTo=" +
                year + "-" + month + "-" + day;
            return dateString;
        }

        //priorityOrder = { 2021, 2016, 2030, 2014, 2077, 2002, 2004, 2019, 2121, 2015, 2142, 2084, 2003, 2017, 2009, 2145, 2137, 2013, 2008, 2024, 2119
        public IEnumerable<FixtureList> reorderFixtureLists(IEnumerable<FixtureList> lists)
        {
            int[] priorityOrder = { 2021, 2016, 2030, 2014, 2077, 2002, 2004, 2019, 2121, 2015, 2142, 2084, 2003, 2017, 2009, 2145, 2137, 2013, 2008, 2024, 2119 };
            return lists.OrderBy(s => Array.IndexOf(priorityOrder, s.leagueId)).ToList();
        }
        
    }
}

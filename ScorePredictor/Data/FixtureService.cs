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
        FixtureList[] orderedList = new FixtureList[21];
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
                // TODO REORDER FIXTURELISTS BASED ON LEAGUE ID
                return fixtureLists;
            }
        }
        private async Task<FixtureList[]> getFixturesFromApi(DateTime? date)
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

                    reorderFixtureLists(fixtureLists);
                    addFixturesToDatabase(orderedList);
                    return orderedList;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        private void addFixturesToDatabase(FixtureList[] lists)
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                foreach(FixtureList list in lists)
                if(list != null)
                {
                        System.Diagnostics.Debug.WriteLine(getDatabaseDateString(dateString) + list.leagueId);
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
                            "@finished, @homescore, @awayscore, @postponed, @utcdate);", connection))
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
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                connection.Close();
            }
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

        public void reorderFixtureLists(List<FixtureList> list)
            //reorder random ordered list into priority order
        {
            if(list.Any())
            {
                foreach(FixtureList i in list)
                {
                    //priorityOrder = { 2021, 2016, 2030, 2014, 2077, 2002, 2004, 2019, 2121, 2015, 2142, 2084, 2003, 2017, 2009, 2145, 2137, 2013, 2008, 2024, 2119
                    switch (i.fixtures[0].leagueId)
                    {
                        case 2021:  //eng
                            orderedList[0] = i;
                            i.imageUrl = "/images/english-flag-small.jpg";
                            System.Diagnostics.Debug.WriteLine(i);
                            break;
                        case 2016:  //eng
                            orderedList[1] = i;
                            i.imageUrl = "/images/english-flag-small.jpg";
                            break;
                        case 2030:  //eng
                            orderedList[2] = i;
                            i.imageUrl = "/images/english-flag-small.jpg";
                            break;
                        case 2084:  //scotland
                            orderedList[11] = i;
                            i.imageUrl = "/images/scotland-flag-large.jpg";
                            break;
                        case 2014:  //spain
                            orderedList[3] = i;
                            i.imageUrl = "/images/spanish-flag-small.jpg";
                            break;
                        case 2077:  //spain
                            orderedList[4] = i;
                            i.imageUrl = "/images/spanish-flag-small.jpg";
                            break;
                        case 2002:  //germany
                            orderedList[5] = i;
                            i.imageUrl = "/images/german-flag-small.jpg";
                            break;
                        case 2004:  //germany
                            orderedList[6] = i;
                            i.imageUrl = "/images/german-flag-small.jpg";
                            break;
                        case 2019:  //italy
                            orderedList[7] = i;
                            i.imageUrl = "/images/italian-flag-small.jpg";
                            break;
                        case 2121:  //italy
                            orderedList[8] = i;
                            i.imageUrl = "/images/italian-flag-small.jpg";
                            break;
                        case 2015:  //france
                            orderedList[9] = i;
                            i.imageUrl = "/images/french-flag-small.jpg";
                            break;
                        case 2142:  //france
                            orderedList[10] = i;
                            i.imageUrl = "/images/french-flag-small.jpg";
                            break;
                        case 2003:  //netherlands
                            orderedList[12] = i;
                            i.imageUrl = "/images/dutch-flag-small.jpg";
                            break;
                        case 2017:  //portugal
                            orderedList[13] = i;
                            i.imageUrl = "/images/portuguese-flag-small.jpg";
                            break;
                        case 2009:  //belgium
                            orderedList[14] = i;
                            i.imageUrl = "/images/belgian-flag-small.jpg";
                            break;
                        case 2145:  //US
                            orderedList[15] = i;
                            i.imageUrl = "/images/american-flag-small.jpg";
                            break;
                        case 2137:  //Russia
                            orderedList[16] = i;
                            i.imageUrl = "/images/russian-flag-small.jpg";
                            break;
                        case 2013:  //Brazil
                            orderedList[17] = i;
                            i.imageUrl = "/images/brazilian-flag-small.jpg";
                            break;
                        case 2008:  //Australia
                            orderedList[18] = i;
                            i.imageUrl = "/images/australian-flag-small.jpg";
                            break;
                        case 2024:  //Argentina
                            orderedList[19] = i;
                            i.imageUrl = "/images/argentinian-flag-small.jpg";
                            break;
                        case 2119:  //Japan
                            orderedList[20] = i;
                            i.imageUrl = "/images/japanese-flag-small.jpg";
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}

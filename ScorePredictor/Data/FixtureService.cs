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

        public async Task<FixtureList[]> getDaysFixtures(DateTime? date)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
            string competitionPriorityOrder = "?competitions=2021,2016,2030,2014,2077,2002,2004,2019,2121,2015,2142,2084,2003,2017,2009,2145,2137,2013,2008,2024,2119";
            string dateString = getDateString(date);

            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches" + competitionPriorityOrder + "&" + dateString))
            {
                if (response.IsSuccessStatusCode)
                {
                    List<FixtureList> fixtureLists = new List<FixtureList>();
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray allMatches = (JArray)jsonObject["matches"];

                    for(int i = 0; i < allMatches.Count; i++)
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
                                    fixture.homeScore = allMatches[i]["score"]["fullTime"]["homeTeam"].ToString();
                                    fixture.awayScore = allMatches[i]["score"]["fullTime"]["awayTeam"].ToString();
                                }
                                else
                                {
                                    fixture.homeScore = "P";
                                    fixture.awayScore = "P";
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
                                fixtureLists.Add(fixtureList);
                            }
                        }
                    }

                    reorderFixtureLists(fixtureLists);
                    return orderedList;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public PredictionStats getPredictionStats(PredictionStats predictionStats)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "scorepredictordb.database.windows.net";
            builder.UserID = "jbest";
            builder.Password = "databasepassword*1";
            builder.InitialCatalog = "scorepredictordb";

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
                predictionStats.correctPercentage = (double)predictionStats.totalCorrect / (double)predictionStats.totalPredictions * 100;
            }
            else
            {
                predictionStats.correctPercentage = 0;
            }
            if(predictionStats.totalCorrectScore > 0)
            {
                predictionStats.correctScorePercentage = (double)predictionStats.totalCorrectScore / (double)predictionStats.totalPredictions * 100;
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

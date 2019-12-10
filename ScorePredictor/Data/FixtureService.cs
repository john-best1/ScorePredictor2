using Newtonsoft.Json.Linq;
using ScorePredictor.Models;
using System;
using System.Collections.Generic;
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

        public async Task<FixtureList[]> getDaysFixtures(string date)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
            string competitionPriorityOrder = "?competitions=2021,2016,2030,2014,2077,2002,2004,2019,2121,2015,2142,2084,2003,2017,2009,2145,2137,2013,2008,2024,2119";
            //string dateString = "&dateFrom=2019-12-14&dateTo=2019-12-14";
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches" + competitionPriorityOrder + "&" + date))
            {
                if (response.IsSuccessStatusCode)
                {
                    List<FixtureList> fixtureLists = new List<FixtureList>();
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray allMatches = (JArray)jsonObject["matches"];

                    for(int i = 0; i < allMatches.Count; i++)
                    {
                        string competitionId = allMatches[i]["competition"]["id"].ToString();
                        Fixture fixture = new Fixture
                        {
                            matchId = int.Parse(allMatches[i]["id"].ToString()),
                            homeTeamId = int.Parse(allMatches[i]["homeTeam"]["id"].ToString()),
                            homeTeamName = allMatches[i]["homeTeam"]["name"].ToString(),
                            awayTeamId = int.Parse(allMatches[i]["awayTeam"]["id"].ToString()),
                            awayTeamName = allMatches[i]["awayTeam"]["name"].ToString(),
                            leagueId = int.Parse(allMatches[i]["competition"]["id"].ToString()),
                            leagueName = allMatches[i]["competition"]["name"].ToString(),
                            utcDate = allMatches[i]["utcDate"].ToString()
                        };
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
                            fixtureList.leagueName = allMatches[i]["competition"]["name"].ToString();
                            fixtureList.utcDate = allMatches[i]["utcDate"].ToString();
                            fixtureList.fixtures = new List<Fixture>();
                            fixtureList.fixtures.Add(fixture);
                            fixtureLists.Add(fixtureList);
                        }
                    }

                    //System.Diagnostics.Debug.WriteLine(fixtureLists);
                    //int[] priorityOrder = { 2021, 2016, 2030, 2014, 2077, 2002, 2004, 2019, 2121, 2015, 2142, 2084, 2003, 2017, 2009, 2145, 2137, 2013, 2008, 2024, 2119 };
                    //priorityOrder.Select(i => fixtureLists[i].fixtures[0].leagueId).ToList();
                    //System.Diagnostics.Debug.WriteLine(priorityOrder);
                    foreach(FixtureList list in fixtureLists)
                    {
                        System.Diagnostics.Debug.WriteLine(list.fixtures[0].leagueName);

                    }
                    reorderFixtureLists(fixtureLists);

                    System.Diagnostics.Debug.WriteLine("--------------");
                    System.Diagnostics.Debug.WriteLine(orderedList);
                    System.Diagnostics.Debug.WriteLine("--------------");
                    return orderedList;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public void reorderFixtureLists(List<FixtureList> list)
            //reorder random ordered list into priority order
        {
            if(list.Any())
            {
                foreach(FixtureList i in list)
                {
                    //priorityOrder = { 2021, 2016, 2030, 2014, 2077, 2002, 2004, 2019, 2121, 2015, 2142, 2084, 2003, 2017, 2009, 2145, 2137, 2013, 2008, 2024, 2119
                    System.Diagnostics.Debug.WriteLine(i.fixtures[0].leagueId);
                    switch (i.fixtures[0].leagueId)
                    {
                        case 2021:
                            orderedList[0] = i;
                            break;
                        case 2016:
                            orderedList[1] = i;
                            break;
                        case 2030:
                            orderedList[2] = i;
                            break;
                        case 2014:
                            orderedList[3] = i;
                            break;
                        case 2077:
                            orderedList[4] = i;
                            break;
                        case 2002:
                            orderedList[5] = i;
                            break;
                        case 2004:
                            orderedList[6] = i;
                            break;
                        case 2019:
                            orderedList[7] = i;
                            break;
                        case 2121:
                            orderedList[8] = i;
                            break;
                        case 2015:
                            orderedList[9] = i;
                            break;
                        case 2142:
                            orderedList[10] = i;
                            break;
                        case 2084:
                            orderedList[11] = i;
                            break;
                        case 2003:
                            orderedList[12] = i;
                            break;
                        case 2017:
                            orderedList[13] = i;
                            break;
                        case 2009:
                            orderedList[14] = i;
                            break;
                        case 2145:
                            orderedList[15] = i;
                            break;
                        case 2137:
                            orderedList[16] = i;
                            break;
                        case 2013:
                            orderedList[17] = i;
                            break;
                        case 2008:
                            orderedList[18] = i;
                            break;
                        case 2024:
                            orderedList[19] = i;
                            break;
                        case 2119:
                            orderedList[20] = i;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}

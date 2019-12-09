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
        public async Task<List<FixtureList>> getDaysFixtures(/*DateTime? date = null*/)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
            string competitionPriorityOrder = "?competitions2021,2016,2030,2014,2077,2002,2004,2019,2121,2015,2142,2084,2003,2017,2009,2145,2137,2013,2008,2024,2119";

            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches" + competitionPriorityOrder))
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
                        System.Diagnostics.Debug.WriteLine(list.fixtures.Count);

                    }

                    return fixtureLists;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }


        }
    }
}

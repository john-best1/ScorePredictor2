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
    public class LeagueService
    {
        public string shortLeagueName { get; set; }
        public string longLeagueName { get; set; }

        HttpClient client = new HttpClient();
        public LeagueService()
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
        }


        public async Task<List<LeagueEntry>> getLeagueTable(int leagueCode = 2016, int leagueTypeCode = 0)
        {
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/competitions/" + leagueCode + "/standings"))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    shortLeagueName = jsonObject["competition"]["name"].ToString();
                    longLeagueName = jsonObject["competition"]["area"]["name"].ToString() + " - " + shortLeagueName;
                    JArray standings = (JArray)jsonObject["standings"];
                    List<LeagueEntry> leagueEntries = new List<LeagueEntry>();
                    for (int i = 0; i < standings[leagueTypeCode]["table"].Count(); i++)
                    {
                        LeagueEntry entry = new LeagueEntry
                        {
                            position = int.Parse(standings[leagueTypeCode]["table"][i]["position"].ToString()),
                            name = standings[leagueTypeCode]["table"][i]["team"]["name"].ToString(),
                            matchesPlayed = int.Parse(standings[leagueTypeCode]["table"][i]["playedGames"].ToString()),
                            won = int.Parse(standings[leagueTypeCode]["table"][i]["won"].ToString()),
                            drawn = int.Parse(standings[leagueTypeCode]["table"][i]["draw"].ToString()),
                            lost = int.Parse(standings[leagueTypeCode]["table"][i]["lost"].ToString()),
                            goalsFor = int.Parse(standings[leagueTypeCode]["table"][i]["goalsFor"].ToString()),
                            goalsAgainst = int.Parse(standings[leagueTypeCode]["table"][i]["goalsAgainst"].ToString()),
                            goalDifference = int.Parse(standings[leagueTypeCode]["table"][i]["goalDifference"].ToString()),
                            points = int.Parse(standings[leagueTypeCode]["table"][i]["points"].ToString())
                        };
                        leagueEntries.Add(entry);
                    }
                    return leagueEntries;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<MatchStats> getStats(int leagueId, int teamId)
        {
            System.Diagnostics.Debug.WriteLine(client.DefaultRequestHeaders);
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/competitions/" + leagueId + "/standings"))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray standings = (JArray)jsonObject["standings"];
                    MatchStats stats = new MatchStats();
                    for (int i = 0; i < standings[0]["table"].Count(); i++)
                    {
                        if (teamId == int.Parse(standings[0]["table"][i]["team"]["id"].ToString()))
                        {
                            stats.position = int.Parse(standings[0]["table"][i]["position"].ToString());
                            stats.name = standings[0]["table"][i]["team"]["name"].ToString();
                            stats.matchesPlayed = int.Parse(standings[0]["table"][i]["playedGames"].ToString());
                            stats.won = int.Parse(standings[0]["table"][i]["won"].ToString());
                            stats.drawn = int.Parse(standings[0]["table"][i]["draw"].ToString());
                            stats.lost = int.Parse(standings[0]["table"][i]["lost"].ToString());
                            stats.goalsFor = int.Parse(standings[0]["table"][i]["goalsFor"].ToString());
                            stats.goalsAgainst = int.Parse(standings[0]["table"][i]["goalsAgainst"].ToString());
                            stats.goalDifference = int.Parse(standings[0]["table"][i]["goalDifference"].ToString());
                            stats.points = int.Parse(standings[0]["table"][i]["points"].ToString());
                            stats.goalsScoredPerGame = stats.goalsFor / stats.matchesPlayed;
                            stats.goalsConcededPerGame = stats.goalsAgainst / stats.matchesPlayed;
                            break;
                        }
                    }
                    return stats;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

    }
}

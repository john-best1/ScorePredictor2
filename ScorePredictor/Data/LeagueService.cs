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
                        if(leagueCode == 2002 || leagueCode == 2021 || leagueCode == 2019 || leagueCode == 2142)
                        {
                            entry.crest = standings[leagueTypeCode]["table"][i]["team"]["crestUrl"].ToString();
                        }
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

        public async Task<MatchStats> getStats(int leagueId, int teamId, string matchId, bool home = true)
        {
            int homeOrAway;
            if (home) homeOrAway = 1;
            else homeOrAway = 2;

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
                            stats.teamId = teamId;
                            stats.matchId = matchId;
                            stats.position = AddOrdinal(int.Parse(standings[0]["table"][i]["position"].ToString()));
                            stats.name = standings[0]["table"][i]["team"]["name"].ToString();
                            stats.matchesPlayed = int.Parse(standings[0]["table"][i]["playedGames"].ToString());
                            stats.won = int.Parse(standings[0]["table"][i]["won"].ToString());
                            stats.drawn = int.Parse(standings[0]["table"][i]["draw"].ToString());
                            stats.lost = int.Parse(standings[0]["table"][i]["lost"].ToString());
                            stats.goalsFor = int.Parse(standings[0]["table"][i]["goalsFor"].ToString());
                            stats.goalsAgainst = int.Parse(standings[0]["table"][i]["goalsAgainst"].ToString());
                            stats.goalDifference = int.Parse(standings[0]["table"][i]["goalDifference"].ToString());
                            stats.points = int.Parse(standings[0]["table"][i]["points"].ToString());
                            stats.goalsScoredPerGame = Math.Round((double)stats.goalsFor / stats.matchesPlayed, 1);
                            stats.goalsConcededPerGame = Math.Round((double)stats.goalsAgainst / stats.matchesPlayed, 1);
                            break;
                        }
                    }
                    for (int i = 0; i < standings[homeOrAway]["table"].Count(); i++)
                    {
                        if (teamId == int.Parse(standings[homeOrAway]["table"][i]["team"]["id"].ToString()))
                        {
                            stats.HomeOrAwayPosition = AddOrdinal(int.Parse(standings[homeOrAway]["table"][i]["position"].ToString()));
                            stats.homeOrAwayWon = int.Parse(standings[homeOrAway]["table"][i]["won"].ToString());
                            stats.homeOrAwayDrawn = int.Parse(standings[homeOrAway]["table"][i]["draw"].ToString());
                            stats.homeOrAwayLost = int.Parse(standings[homeOrAway]["table"][i]["lost"].ToString());
                            stats.homeOrAwayMatchesPlayed = int.Parse(standings[homeOrAway]["table"][i]["playedGames"].ToString());
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

        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }

    }
}

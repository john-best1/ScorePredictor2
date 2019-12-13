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
    public class MatchService
    {
        Match match = new Match();
        LeagueService leagueService = new LeagueService();
        HttpClient client = new HttpClient();
        int season = 2019;   //change this every season

        public async Task<Match> getMatch(int matchId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");

            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/matches/" + matchId))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                    populateMatch(jsonObject);

                    match.HomeStats = await leagueService.getStats(match.LeagueId, match.HomeTeamId);
                    match.AwayStats = await leagueService.getStats(match.LeagueId, match.AwayTeamId);
                    getWDLString(match.HomeStats);
                    getWDLString(match.AwayStats);
                    return match;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task<Match> getRecentForm(Match match)
        {
            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/competitions/" + match.LeagueId + "/matches?status=FINISHED&season=" + season ))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray allCompetitionMatches = (JArray)jsonObject["matches"];
                    match.HomeStats.overallLastSix = (populateOverallLast6(match.LeagueId, match.HomeTeamId, allCompetitionMatches));
                    match.AwayStats.overallLastSix = (populateOverallLast6(match.LeagueId, match.AwayTeamId, allCompetitionMatches));
                    return match;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        private void getWDLString(MatchStats stats)
        {
            string won = Math.Round((double)stats.won / stats.matchesPlayed * 100.0, 0).ToString();
            string drawn = Math.Round((double)stats.drawn / stats.matchesPlayed * 100.0, 0).ToString();
            string lost = Math.Round((double)stats.lost / stats.matchesPlayed * 100.0, 0).ToString();
            stats.WDL = won + "/" + drawn + "/" + lost;
        }

        private void populateMatch(JObject jsonObject)
        {
            match.Stadium = jsonObject["match"]["venue"].ToString();

            match.HomeTeamName = jsonObject["match"]["homeTeam"]["name"].ToString();
            match.HomeTeamId = int.Parse(jsonObject["match"]["homeTeam"]["id"].ToString());
            match.AwayTeamName = jsonObject["match"]["awayTeam"]["name"].ToString();
            match.AwayTeamId = int.Parse(jsonObject["match"]["awayTeam"]["id"].ToString());
            match.MatchId = jsonObject["match"]["id"].ToString();
            match.UtcDate = jsonObject["match"]["utcDate"].ToString();
            match.LeagueId = int.Parse(jsonObject["match"]["competition"]["id"].ToString());
            match.LeagueName = jsonObject["match"]["competition"]["name"].ToString();
        }

        public int[] populateOverallLast6(int leagueId, int teamId, JArray allMatches)
        {
            //1 = home win, 2 = away win, 3 = draw, 4 = loss
            int[] results = new int[6];
            int added = 0;
            for(int i = 0; i < allMatches.Count; i++)
            {
                if(int.Parse(allMatches[i]["homeTeam"]["id"].ToString()) == teamId)
                {
                    switch(allMatches[i]["score"]["winner"].ToString())
                    {
                        case "HOME_TEAM":
                            results[5 - added] = 1;
                            added++;
                            break;
                        case "DRAW":
                            results[5 - added] = 3;
                            added++;
                            break;
                        default:
                            results[5 - added] = 4;
                            added++;
                            break;
                    }
                }
                else if (int.Parse(allMatches[i]["awayTeam"]["id"].ToString()) == teamId)
                {
                    switch (allMatches[i]["score"]["winner"].ToString())
                    {
                        case "AWAY_TEAM":
                            results[5 - added] = 2;
                            added++;
                            break;
                        case "DRAW":
                            results[5 - added] = 3;
                            added++;
                            break;
                        default:
                            results[5 - added] = 4;
                            added++;
                            break;
                    }
                }
                if (added == 6) break;
            }
            return results;
        }
    }
}

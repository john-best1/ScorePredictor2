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
        public async Task<List<LeagueEntry>> getLeagueTable(int leagueCode = 2017)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");

            using (HttpResponseMessage response = await client.GetAsync("https://api.football-data.org/v2/competitions/" + leagueCode + "/standings"))
            {
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                    JArray standings = (JArray)jsonObject["standings"];
                    List<LeagueEntry> leagueEntries = new List<LeagueEntry>();
                    for (int i = 0; i < standings[0]["table"].Count(); i++)
                    {
                        LeagueEntry entry = new LeagueEntry
                        {
                            position = int.Parse(standings[0]["table"][i]["position"].ToString()),
                            name = standings[0]["table"][i]["team"]["name"].ToString(),
                            matchesPlayed = int.Parse(standings[0]["table"][i]["playedGames"].ToString()),
                            won = int.Parse(standings[0]["table"][i]["won"].ToString()),
                            drawn = int.Parse(standings[0]["table"][i]["draw"].ToString()),
                            lost = int.Parse(standings[0]["table"][i]["lost"].ToString()),
                            goalsFor = int.Parse(standings[0]["table"][i]["goalsFor"].ToString()),
                            goalsAgainst = int.Parse(standings[0]["table"][i]["goalsAgainst"].ToString()),
                            goalDifference = int.Parse(standings[0]["table"][i]["goalDifference"].ToString()),
                            points = int.Parse(standings[0]["table"][i]["points"].ToString())
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
    }
}


//var request = new HttpRequestMessage()
//{
//    RequestUri = new Uri("https://api.football-data.org/v2/competitions/" + leagueCode + "/standings"),
//    Method = HttpMethod.Get,
//};
//request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue({ 'X-Auth-Token', '7830c352850f4acda78aa61d1666d45b' }))
//var task = client.SendAsync

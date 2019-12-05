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
    public static class LeagueService
    {
        static async Task<object> getLeagueTable(int leagueCode)
        {

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Token", "7830c352850f4acda78aa61d1666d45b");
            //var request = new HttpRequestMessage()
            //{
            //    RequestUri = new Uri("https://api.football-data.org/v2/competitions/" + leagueCode + "/standings"),
            //    Method = HttpMethod.Get,
            //};
            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue({ 'X-Auth-Token', '7830c352850f4acda78aa61d1666d45b' }))
            //var task = client.SendAsync
            return await client.GetAsync("https://api.football-data.org/v2/competitions/" + leagueCode + "/standings");
        }
    }
}

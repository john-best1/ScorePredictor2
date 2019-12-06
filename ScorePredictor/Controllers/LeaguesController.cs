using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScorePredictor.Data;
using ScorePredictor.Models;

namespace ScorePredictor.Controllers
{
    public class LeaguesController : Controller
    {
        public async Task<IActionResult> Leagues(LeagueService leagueService)
        {
            List<LeagueEntry> leagues = await leagueService.getLeagueTable(2021);

            System.Diagnostics.Debug.WriteLine("------------");
            System.Diagnostics.Debug.WriteLine(leagues[0]);
            System.Diagnostics.Debug.WriteLine("------------");

            return View();
        }
    }
}
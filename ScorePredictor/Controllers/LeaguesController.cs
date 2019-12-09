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
            List<LeagueEntry> leagues = await leagueService.getLeagueTable();

            System.Diagnostics.Debug.WriteLine("------------");
            System.Diagnostics.Debug.WriteLine(leagues.GetType());
            System.Diagnostics.Debug.WriteLine("------------");

            foreach(LeagueEntry entry in leagues)
            {
                System.Diagnostics.Debug.WriteLine(entry.name);
            }
            ViewBag.Message = leagueService.leagueName;
            return View(leagues);
        }
    }
}
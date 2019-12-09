using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScorePredictor.Data;
using ScorePredictor.Models;
using ScorePredictor.ViewModels;

namespace ScorePredictor.Controllers
{
    public class LeaguesController : Controller
    {
        public async Task<IActionResult> Leagues(LeagueService leagueService, int leagueCode = 2021, int leagueTypeCode = 0)
        {
            LeaguesViewModel viewModel = new LeaguesViewModel(leagueCode, leagueTypeCode);
            List<LeagueEntry> leagues = await leagueService.getLeagueTable(leagueCode, leagueTypeCode);
            viewModel.leagues = leagues;
            viewModel.shortLeagueTitle = leagueService.shortLeagueName;
            viewModel.longLeagueTitle = leagueService.longLeagueName;
            return View(viewModel);
        }
    }
}
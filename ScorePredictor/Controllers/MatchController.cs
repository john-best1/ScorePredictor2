﻿using Microsoft.AspNetCore.Mvc;
using ScorePredictor.Data;
using ScorePredictor.Models;
using ScorePredictor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScorePredictor.Controllers
{
    public class MatchController : Controller
    {
        public async Task<IActionResult> Match(MatchService matchService, LeagueService leagueService,string matchId, string flagUrl, bool matchFinished)
        {
            Match match = await matchService.getMatch(matchId, flagUrl);
            MatchViewModel viewModel = new MatchViewModel(match);
            return View(viewModel);
        }
    }
}

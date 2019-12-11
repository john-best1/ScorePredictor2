using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScorePredictor.Data;
using ScorePredictor.Models;
using ScorePredictor.ViewModels;

namespace ScorePredictor.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(FixtureService fixtureService, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now.AddDays(2);
            }

            FixtureList[] fixtureLists = await fixtureService.getDaysFixtures(date);
            FixturesViewModel viewModel = new FixturesViewModel { fixtureLists = fixtureLists, date = date};

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

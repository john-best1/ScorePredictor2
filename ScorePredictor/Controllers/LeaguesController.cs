using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ScorePredictor.Controllers
{
    public class LeaguesController : Controller
    {
        public IActionResult Leagues()
        {
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ScorePredictor.Data;
using ScorePredictor.Models;
using ScorePredictor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScorePredictor.Controllers
{
    public class DataController : Controller
    {

        public IActionResult Data(DataService dataService)
        {
            PredictionData predictionData = dataService.getPredictionData();
            DataViewModel viewModel = new DataViewModel(predictionData);
            return View(viewModel);
        }
    }
}

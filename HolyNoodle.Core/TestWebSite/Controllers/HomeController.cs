using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HolyNoodle.Core.Utility.TestWebSite.Models;

namespace HolyNoodle.Core.Utility.TestWebSite.Controllers
{
    public class HomeController : Controller
    {
        private ILocalisationService _localisationService;

        public HomeController(ILocalisationService localisationService)
        {
            _localisationService = localisationService;
        }

        public async Task<IActionResult> Index()
        {
            var test = await _localisationService.GetText("Name");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

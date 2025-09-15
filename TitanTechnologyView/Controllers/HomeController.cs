using System.Diagnostics;
using internalPortalFroent.Models;
using Microsoft.AspNetCore.Mvc;

namespace internalPortalFroent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Careers()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Solutions()
        {
            return View();
        }

        public PartialViewResult Header() 
        {
            return PartialView("_Header");
        }

        public PartialViewResult Footer()
        {
            return PartialView("_Footer");
        }      

        public IActionResult Privacy()
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

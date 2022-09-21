using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Couchbase.Extensions.Session.Example.Models;
using Couchbase.Query.Couchbase.N1QL;

namespace Couchbase.Extensions.Session.Example.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
           // await HttpContext.Session.LoadAsync();
           // HttpContext.Session.Set("thekey",
              //  System.Text.Encoding.UTF8.GetBytes("{ \"name\" : \"Session stored in couchbase!\"}"));
            await HttpContext.Session.SetObject("theKey", "{ \"name\" : \"Session stored in couchbase!\"}").ConfigureAwait(false);
            return View(); ;
        }

        public async Task<IActionResult> Privacy()
        {
            ViewData["Message"] = await HttpContext.Session.GetObject<string>("theKey").ConfigureAwait(false);
            return View();
        }

        public ActionResult Clear()
        {
            HttpContext.Session.Remove("theKey");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

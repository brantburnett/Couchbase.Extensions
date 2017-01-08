using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestApp.Buckets;
using TestApp.Models;

namespace TestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITravelSampleBucketProvider _bucketProvider;

        public HomeController(ITravelSampleBucketProvider bucketProvider)
        {
            _bucketProvider = bucketProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Airlines()
        {
            var bucket = _bucketProvider.GetBucket();

            var result =
                await bucket.QueryAsync<Airline>(
                    "SELECT Extent.* FROM `travel-sample` AS Extent WHERE type = 'airline' ORDER BY name");

            if (!result.Success)
            {
                throw new Exception("Couchbase Error", result.Exception);
            }

            return View(result.Rows);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

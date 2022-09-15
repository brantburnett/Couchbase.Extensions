using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Couchbase.Extensions.Caching.Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private IDistributedCache _cache;

        public WeatherForecastController(IDistributedCache cache, ILogger<WeatherForecastController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private static readonly string[] Summaries = new[]
            {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var weatherForcast = await _cache.GetAsync<IEnumerable<WeatherForecast>>("weatherForecast");

            if(weatherForcast == null)
            {
                _logger.LogInformation("Cache miss!");
                weatherForcast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                }).ToArray();

                await _cache.SetAsync("weatherForecast", weatherForcast,
                    new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10)));

                return weatherForcast;
            }
            _logger.LogInformation("Cache hit!");
            return weatherForcast;
        }
    }
}
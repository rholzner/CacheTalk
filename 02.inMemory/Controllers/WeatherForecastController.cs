using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace _02.inMemory.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };


    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get(IMemoryCache memoryCache, ILogger<WeatherForecastController> logger)
    {
        //return await DoWork(logger);
        return await DoWorkCached(memoryCache, logger);
    }

    private async ValueTask<IEnumerable<WeatherForecast>> DoWork(ILogger<WeatherForecastController> logger)
    {
        logger.LogDebug("DoWork:Getting data");
        var getData = Enumerable.Range(1, 10).Select(async index =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }).ToArray();

        Task.WaitAll(getData);
        logger.LogDebug("DoWork:Done getting data");

        List<WeatherForecast> r = new ();

        foreach (var item in getData)
        {
            r.Add(item.Result);
        }

        return r;
    }

    #region cached
    private async ValueTask<IEnumerable<WeatherForecast>> DoWorkCached(IMemoryCache memoryCache, ILogger<WeatherForecastController> logger)
    {
        if (memoryCache.TryGetValue("DoWorkCached", out IEnumerable<WeatherForecast>? cachedData) && cachedData is not null)
        {
            return cachedData;
        }

        var r = await DoWork(logger);

        memoryCache.Set("DoWorkCached",r);

        return r;
    }
    #endregion

}

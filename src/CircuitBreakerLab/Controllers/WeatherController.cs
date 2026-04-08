using CircuitBreakerLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace CircuitBreakerLab.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get(CancellationToken cancellationToken)
    {
        return await _weatherService.GetForecastAsync(cancellationToken);
    }
}

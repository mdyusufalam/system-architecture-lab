using Microsoft.AspNetCore.Mvc;
using ResiallianceCircuitBreakerLab.Services;

namespace ResiallianceCircuitBreakerLab.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetWeatherForecast()
    {
        try
        {
            var forecast = await _weatherService.GetWeatherForecastAsync();
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather forecast");
            return StatusCode(500, "Internal server error");
        }
    }
}
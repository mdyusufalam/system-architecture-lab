using Microsoft.AspNetCore.Mvc;
using ObservabilityPoc.Services;

namespace ObservabilityPoc.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly WeatherService _weatherService;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(WeatherService weatherService, ILogger<WeatherForecastController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [HttpGet("fast")]
    public async Task<IActionResult> GetFast()
    {
        _logger.LogInformation("Received fast weather request.");
        var data = await _weatherService.GetWeatherAsync(delay: false, throwError: false);
        return Ok(data);
    }

    [HttpGet("slow")]
    public async Task<IActionResult> GetSlow()
    {
        _logger.LogInformation("Received slow weather request.");
        var data = await _weatherService.GetWeatherAsync(delay: true, throwError: false);
        return Ok(data);
    }

    [HttpGet("error")]
    public async Task<IActionResult> GetError()
    {
        _logger.LogWarning("Received error weather request. About to throw...");
        try
        {
            await _weatherService.GetWeatherAsync(delay: false, throwError: true);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = ex.Message });
        }
    }
}

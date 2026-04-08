using System.Net;

namespace ResiallianceCircuitBreakerLab.Clients;

public class ExternalWeatherClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalWeatherClient> _logger;
    private readonly Random _random = new();

    public ExternalWeatherClient(HttpClient httpClient, ILogger<ExternalWeatherClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetWeatherDataAsync()
    {
        // Simulate external service failure 40% of the time
        if (_random.Next(0, 100) < 40)
        {
            _logger.LogWarning("Simulating external weather service failure");
            throw new HttpRequestException("External weather service is unavailable", null, HttpStatusCode.ServiceUnavailable);
        }

        // Simulate successful response
        var temperature = _random.Next(-20, 55);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var summary = summaries[_random.Next(summaries.Length)];

        _logger.LogInformation($"Fetched weather data: {temperature}°C, {summary}");
        return $"{temperature}°C, {summary}";
    }
}
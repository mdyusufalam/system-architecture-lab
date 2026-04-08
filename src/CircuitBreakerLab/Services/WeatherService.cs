using System.Collections.Generic;
using CircuitBreakerLab.Clients;
using Polly.CircuitBreaker;

namespace CircuitBreakerLab.Services;

public class WeatherService : IWeatherService
{
    private readonly ExternalWeatherClient _externalWeatherClient;

    public WeatherService(ExternalWeatherClient externalWeatherClient)
    {
        _externalWeatherClient = externalWeatherClient;
    }

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(CancellationToken cancellationToken = default)
    {
        var forecasts = new List<WeatherForecast>();

        for (var index = 1; index <= 5; index++)
        {
            try
            {
                var weather = await _externalWeatherClient.GetRemoteWeatherAsync(cancellationToken);
                forecasts.Add(weather with { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)) });
            }
            catch (BrokenCircuitException)
            {
                forecasts.Add(new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                    0,
                    "Service unavailable - circuit breaker is open"));
            }
            catch (Exception ex)
            {
                forecasts.Add(new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                    0,
                    "Service unavailable - transient failure: " + ex.Message));
            }
        }

        return forecasts;
    }
}

public sealed record WeatherForecast(DateOnly Date, int TemperatureC, string Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

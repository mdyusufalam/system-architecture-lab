using Polly;
using Polly.CircuitBreaker;
using ResiallianceCircuitBreakerLab.Clients;
using ResiallianceCircuitBreakerLab.Policies;

namespace ResiallianceCircuitBreakerLab.Services;

public class WeatherService : IWeatherService
{
    private readonly ExternalWeatherClient _externalClient;
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ExternalWeatherClient externalClient, ILogger<WeatherService> logger)
    {
        _externalClient = externalClient;
        _logger = logger;
        _resiliencePolicy = ResiliencePolicies.CreateResiliencePolicy(logger);
    }

    public async Task<WeatherForecast[]> GetWeatherForecastAsync()
    {
        try
        {
            // Use the resilience policy to call the external service
            var weatherData = await _resiliencePolicy.ExecuteAsync(() => _externalClient.GetWeatherDataAsync());

            // Parse and create forecast
            var forecasts = new List<WeatherForecast>();
            for (int i = 1; i <= 5; i++)
            {
                // For demo, use the same data for all days, or parse if multiple
                var parts = weatherData.Split(',');
                if (parts.Length >= 2 && int.TryParse(parts[0].Trim().Replace("°C", ""), out var temp))
                {
                    forecasts.Add(new WeatherForecast(
                        DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                        temp,
                        parts[1].Trim()));
                }
                else
                {
                    // Fallback
                    forecasts.Add(new WeatherForecast(
                        DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                        20,
                        "Mild"));
                }
            }

            return forecasts.ToArray();
        }
        catch (BrokenCircuitException)
        {
            _logger.LogError("Circuit breaker is open, returning fallback response");
            // Return fallback data when circuit is open
            return new[]
            {
                new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 20, "Service unavailable - circuit breaker is open"),
                new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), 20, "Service unavailable - circuit breaker is open"),
                new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(3)), 20, "Service unavailable - circuit breaker is open"),
                new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(4)), 20, "Service unavailable - circuit breaker is open"),
                new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(5)), 20, "Service unavailable - circuit breaker is open")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in weather service");
            throw;
        }
    }
}
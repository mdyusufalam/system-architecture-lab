using System.Net.Http;
using CircuitBreakerLab.Services;
using Polly.CircuitBreaker;

namespace CircuitBreakerLab.Clients;

public class ExternalWeatherClient
{
    private readonly AsyncCircuitBreakerPolicy _breaker;

    public ExternalWeatherClient(AsyncCircuitBreakerPolicy breaker)
    {
        _breaker = breaker;
    }

    public Task<WeatherForecast> GetRemoteWeatherAsync(CancellationToken cancellationToken = default)
    {
        return _breaker.ExecuteAsync(async ct =>
        {
            await Task.Delay(150, ct);

            if (Random.Shared.NextDouble() < 0.4)
            {
                throw new HttpRequestException("Simulated external weather service failure.");
            }

            return new WeatherForecast(
                DateOnly.FromDateTime(DateTime.UtcNow),
                Random.Shared.Next(-8, 35),
                "Cloudy with Pollys of resilience");
        }, cancellationToken);
    }
}

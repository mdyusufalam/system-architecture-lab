using System.Collections.Generic;
using System.Threading;

namespace CircuitBreakerLab.Services;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(CancellationToken cancellationToken = default);
}

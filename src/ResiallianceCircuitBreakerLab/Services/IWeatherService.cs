namespace ResiallianceCircuitBreakerLab.Services;

public interface IWeatherService
{
    Task<WeatherForecast[]> GetWeatherForecastAsync();
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
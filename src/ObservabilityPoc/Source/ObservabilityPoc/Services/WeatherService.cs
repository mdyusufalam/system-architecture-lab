using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Trace;

namespace ObservabilityPoc.Services;

public class WeatherService
{
    public static readonly ActivitySource ActivitySource = new("ObservabilityPoc.Services.WeatherService");
    public static readonly Meter Meter = new("ObservabilityPoc.Services.WeatherService");
    
    private readonly Counter<int> _weatherRequestsCounter;
    private readonly Histogram<double> _weatherProcessingLatency;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
        _weatherRequestsCounter = Meter.CreateCounter<int>("weather.requests.count", description: "Counts the number of weather requests");
        _weatherProcessingLatency = Meter.CreateHistogram<double>("weather.processing.latency", "ms", "Measures the latency of weather data processing");
    }

    public async Task<string[]> GetWeatherAsync(bool delay, bool throwError)
    {
        using var activity = ActivitySource.StartActivity("ProcessWeatherRequest");
        activity?.SetTag("request.delay", delay);
        activity?.SetTag("request.throwError", throwError);

        var stopwatch = Stopwatch.StartNew();
        _weatherRequestsCounter.Add(1);

        _logger.LogInformation("Processing weather request. Delay: {Delay}, ThrowError: {ThrowError}", delay, throwError);

        try
        {
            if (delay)
            {
                using var delayActivity = ActivitySource.StartActivity("SimulateDelay");
                await Task.Delay(Random.Shared.Next(100, 500));
            }

            if (throwError)
            {
                throw new InvalidOperationException("Simulated catastrophic weather failure.");
            }

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            return summaries;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("Exception", tags: new ActivityTagsCollection { { "exception.message", ex.Message } }));
            _logger.LogError(ex, "Failed to process weather request: {Message}", ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _weatherProcessingLatency.Record(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}

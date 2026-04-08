using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ResiallianceCircuitBreakerLab.Policies;

public static class ResiliencePolicies
{
    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to: {exception.Message}");
                });
    }

    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(ILogger logger)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(15),
                onBreak: (exception, breakDelay) =>
                {
                    logger.LogWarning($"Circuit breaker opened for {breakDelay.TotalSeconds} seconds due to: {exception.Message}");
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open, testing service");
                });
    }

    public static IAsyncPolicy CreateResiliencePolicy(ILogger logger)
    {
        var retryPolicy = CreateRetryPolicy(logger);
        var circuitBreakerPolicy = CreateCircuitBreakerPolicy(logger);

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }
}
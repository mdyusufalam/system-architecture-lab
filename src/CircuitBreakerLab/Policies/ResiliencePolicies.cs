using Polly;
using Polly.CircuitBreaker;

namespace CircuitBreakerLab.Policies;

public static class ResiliencePolicies
{
    public static AsyncCircuitBreakerPolicy GetWeatherCircuitBreakerPolicy()
    {
        return Policy
            .Handle<Exception>()
            
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(15),
                onBreak: (exception, breakDelay) =>
                {
                    Console.WriteLine($"[CircuitBreaker] Open for {breakDelay.TotalSeconds}s because: {exception.Message}");
                },
                onReset: () => Console.WriteLine("[CircuitBreaker] Reset."),
                onHalfOpen: () => Console.WriteLine("[CircuitBreaker] Half-open; next call is a trial."));
    }
}

# Resilience Circuit Breaker Lab

A demonstration project showcasing resilience patterns using Polly library in .NET, implementing both Retry and Circuit Breaker policies.

## What It Does

This project simulates a weather forecasting service that relies on an external weather API. Resilience patterns are implemented using Polly to handle transient failures gracefully:

- **Retry Policy**: Automatically retries failed requests up to 3 times with exponential backoff (2^attempt seconds).
- **Circuit Breaker**: After 2 consecutive failures, the circuit breaker opens for 15 seconds, preventing further calls to the failing service.
- **Combined Resilience**: The retry policy wraps the circuit breaker, providing comprehensive fault tolerance.

## Resilience Flow

1. **Normal Operation**: The service fetches weather data from the simulated external client.
2. **Retry on Failure**: When the external service fails, the retry policy attempts up to 3 retries with increasing delays.
3. **Circuit Breaker Activation**: If retries continue to fail (2 consecutive failures), the circuit breaker opens.
4. **Fallback Response**: During open circuit, the service returns a fallback message instead of attempting the call.
5. **Recovery**: After the break duration, the circuit enters half-open state, allowing a trial call to test if the service has recovered.

## Project Structure

```
src/
└── ResiallianceCircuitBreakerLab/
    ├── Controllers/
    │   └── WeatherController.cs      # API endpoint for weather forecasts
    ├── Services/
    │   ├── IWeatherService.cs        # Weather service interface
    │   └── WeatherService.cs         # Weather service implementation with resilience
    ├── Clients/
    │   └── ExternalWeatherClient.cs  # Simulated external weather API client
    ├── Policies/
    │   └── ResiliencePolicies.cs     # Retry and circuit breaker policy configuration
    └── Program.cs                    # Application startup and DI configuration
```

## Prerequisites

- .NET 10 SDK (installed and available in PATH)

## How to Run

1. **Navigate to the project directory**:
   ```
   cd c:\Users\mdyus\source\repos\SystemArchitectLab
   ```

2. **Build the project**:
   ```
   dotnet build src/ResiallianceCircuitBreakerLab/ResiallianceCircuitBreakerLab.csproj
   ```

3. **Run the application**:
   ```
   dotnet run --project src/ResiallianceCircuitBreakerLab/ResiallianceCircuitBreakerLab.csproj
   ```

4. **Access the API**:
   - Open a browser or use a tool like curl/Postman
   - Visit: `http://localhost:5000/weather/forecast`
   - The endpoint returns a 5-day weather forecast

## Testing the Resilience Patterns

- **Normal Response**: Most calls will return weather data with random temperatures.
- **Failure Simulation**: Approximately 40% of calls will simulate external service failures.
- **Retry Behavior**: Failed calls will show retry attempts in the console logs with increasing delays.
- **Circuit Breaker Activation**: After multiple failed retries, you'll see "Circuit breaker opened" messages, followed by fallback responses.
- **Recovery**: Wait 15 seconds, then try again - the circuit will attempt to recover.

## Dependencies

- **Polly**: Resilience and transient-fault-handling library for .NET
- **ASP.NET Core**: Web framework for building the API

## Configuration

The resilience policies are configured in `Policies/ResiliencePolicies.cs`:
- **Retry Policy**:
  - Retry count: 3 attempts
  - Backoff strategy: Exponential (2^attempt seconds)
- **Circuit Breaker**:
  - Exceptions allowed before breaking: 2
  - Break duration: 15 seconds
  - Logging: Console output for state changes

You can modify these values to experiment with different resilience behaviors.
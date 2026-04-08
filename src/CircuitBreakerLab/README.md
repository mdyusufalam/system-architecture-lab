# Circuit Breaker Lab

A demonstration project showcasing the Circuit Breaker pattern using Polly resilience library in .NET 10.

## What It Does

This project simulates a weather forecasting service that relies on an external weather API. The Circuit Breaker pattern is implemented using Polly to handle transient failures gracefully:

- **Normal Operation**: The service fetches weather data from the simulated external client.
- **Failure Handling**: When the external service fails (simulated with a 40% failure rate), the circuit breaker tracks failures.
- **Circuit Breaker Opens**: After 2 consecutive failures, the circuit breaker opens for 15 seconds, preventing further calls to the failing service.
- **Fallback Response**: During open circuit, the service returns a fallback message instead of attempting the call.
- **Recovery**: After the break duration, the circuit enters half-open state, allowing a trial call to test if the service has recovered.

## Project Structure

```
src/
└── CircuitBreakerLab/
    ├── Controllers/
    │   └── WeatherController.cs      # API endpoint for weather forecasts
    ├── Services/
    │   ├── IWeatherService.cs        # Weather service interface
    │   └── WeatherService.cs         # Weather service implementation
    ├── Clients/
    │   └── ExternalWeatherClient.cs  # Simulated external weather API client
    ├── Policies/
    │   └── ResiliencePolicies.cs     # Circuit breaker policy configuration
    └── Program.cs                    # Application startup and DI configuration
```

## Prerequisites

- .NET 10 SDK (installed and available in PATH)

## How to Run

1. **Clone or navigate to the project directory**:
   ```
   cd c:\Users\mdyus\source\repos\SystemArchitectLab
   ```

2. **Build the project**:
   ```
   dotnet build src/CircuitBreakerLab/CircuitBreakerLab.csproj
   ```

3. **Run the application**:
   ```
   dotnet run --project src/CircuitBreakerLab/CircuitBreakerLab.csproj
   ```

4. **Access the API**:
   - Open a browser or use a tool like curl/Postman
   - Visit: `http://localhost:5000/weather`
   - The endpoint returns a 5-day weather forecast

## Testing the Circuit Breaker

- **Normal Response**: Most calls will return weather data with random temperatures.
- **Failure Simulation**: Approximately 40% of calls will simulate external service failures.
- **Circuit Breaker Activation**: After 2-3 failed calls, you'll see "Service unavailable - circuit breaker is open" messages.
- **Recovery**: Wait 15 seconds, then try again - the circuit will attempt to recover.

## Dependencies

- **Polly**: Resilience and transient-fault-handling library for .NET
- **ASP.NET Core**: Web framework for building the API

## Configuration

The circuit breaker policy is configured in `Policies/ResiliencePolicies.cs`:
- Exceptions allowed before breaking: 2
- Break duration: 15 seconds
- Logging: Console output for circuit state changes

You can modify these values to experiment with different circuit breaker behaviors.
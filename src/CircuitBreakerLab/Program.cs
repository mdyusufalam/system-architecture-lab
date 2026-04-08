using CircuitBreakerLab.Clients;
using CircuitBreakerLab.Policies;
using CircuitBreakerLab.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton(ResiliencePolicies.GetWeatherCircuitBreakerPolicy());
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<ExternalWeatherClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

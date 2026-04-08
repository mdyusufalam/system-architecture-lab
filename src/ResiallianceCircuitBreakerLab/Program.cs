using ResiallianceCircuitBreakerLab.Clients;
using ResiallianceCircuitBreakerLab.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register HttpClient for ExternalWeatherClient
builder.Services.AddHttpClient<ExternalWeatherClient>();

// Register weather service
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

using MarketingPoc.Data;
using MarketingPoc.Repositories;
using MarketingPoc.Services;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// builder.Services.Configure<KestrelServerOptions>(options =>
// {
//     options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(5);
// });
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
});

builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();
builder.Services.AddScoped<SeederService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var seeder = scope.ServiceProvider.GetRequiredService<SeederService>();
    
    logger.LogInformation("Starting database seeding...");
    try
    {
        var seeded = await seeder.SeedIfEmptyAsync();
        logger.LogInformation("Database seeding completed. Data seeded: {Seeded}", seeded);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database seeding");
    }
}

app.Run();

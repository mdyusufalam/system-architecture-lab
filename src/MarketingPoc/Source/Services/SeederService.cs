using MarketingPoc.Data;
using MarketingPoc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MarketingPoc.Services;

public class SeederService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SeederService> _logger;

    public SeederService(AppDbContext dbContext, IConfiguration configuration, ILogger<SeederService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SeedIfEmptyAsync()
    {
        _logger.LogInformation("Running database migration...");
        await _dbContext.Database.MigrateAsync();
        _logger.LogInformation("Database migration completed.");

        _logger.LogInformation("Checking if database already has data...");
        var existingTenantCount = await _dbContext.Tenants.CountAsync();
        _logger.LogInformation("Found {TenantCount} tenants in database", existingTenantCount);
        // Temporarily force seeding
        // if (existingTenantCount > 0)
        // {
        //     _logger.LogInformation("Database already has data, skipping seeding.");
        //     return false;
        // }

        int tenantCount = _configuration.GetValue<int>("Seed:TenantCount");
        int campaignCount = _configuration.GetValue<int>("Seed:CampaignCount");
        _logger.LogInformation("Seeding {TenantCount} tenants and {CampaignCount} campaigns", tenantCount, campaignCount);

        var connectionString = _dbContext.Database.GetConnectionString();

        var tenants = Enumerable.Range(1, tenantCount)
            .Select(i => new Tenant
            {
                Id = i,
                Name = $"Tenant {i}",
                RateLimit = 500
            })
            .ToList();

        _logger.LogInformation("Seeding {TenantCount} tenants...", tenants.Count);
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
 if (existingTenantCount <1000)
         {
        await using (var writer = connection.BeginBinaryImport("COPY \"Tenants\" (\"Id\", \"Name\", \"RateLimit\") FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var tenant in tenants)
            {
                writer.StartRow();
                writer.Write(tenant.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                writer.Write(tenant.Name, NpgsqlTypes.NpgsqlDbType.Text);
                writer.Write(tenant.RateLimit, NpgsqlTypes.NpgsqlDbType.Integer);
            }
            await writer.CompleteAsync();
        }
        _logger.LogInformation("Tenants seeded successfully.");
            }

        // Check if campaigns already exist
        var existingCampaignCount = await _dbContext.Campaigns.CountAsync();
        _logger.LogInformation("Found {CampaignCount} campaigns in database", existingCampaignCount);
        
        if (existingCampaignCount >= campaignCount)
        {
            _logger.LogInformation("Database already has all campaigns, skipping campaign seeding.");
        }
        else
        {
            int campaignsToSeed = campaignCount - existingCampaignCount;
            _logger.LogInformation("Seeding {CampaignsToSeed} additional campaigns...", campaignsToSeed);
            const int batchSize = 100000; // Process in batches of 100k to avoid timeouts
            var random = new Random();
            var channels = new[] { "Email", "Social", "Search", "Display", "SMS" };
            var statuses = new[] { "Scheduled", "Running", "Paused", "Completed" };

            for (int batchStart = 0; batchStart < campaignsToSeed; batchStart += batchSize)
            {
                int currentBatchSize = Math.Min(batchSize, campaignsToSeed - batchStart);
                _logger.LogInformation("Seeding campaigns batch {BatchStart} to {BatchEnd} ({CurrentBatchSize} records)",
                    existingCampaignCount + batchStart + 1, existingCampaignCount + batchStart + currentBatchSize, currentBatchSize);

                await using (var writer = connection.BeginBinaryImport("COPY \"Campaigns\" (\"TenantId\", \"Channel\", \"Content\", \"Status\", \"ScheduledAt\") FROM STDIN (FORMAT BINARY)"))
                {
                    for (int i = 0; i < currentBatchSize; i++)
                    {
                        int globalIndex = existingCampaignCount + batchStart + i;
                        writer.StartRow();
                        int tenantId = (globalIndex % tenantCount) + 1;
                        writer.Write(tenantId, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(channels[random.Next(channels.Length)], NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write($"Campaign content for tenant {tenantId} - batch {globalIndex + 1}", NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(statuses[random.Next(statuses.Length)], NpgsqlTypes.NpgsqlDbType.Text);
                        writer.Write(DateTime.UtcNow.AddMinutes(random.Next(-1440, 1440)), NpgsqlTypes.NpgsqlDbType.TimestampTz);
                    }
                    await writer.CompleteAsync();
                }
            }
            _logger.LogInformation("Campaigns seeded successfully.");
        }

        return true;
    }
}
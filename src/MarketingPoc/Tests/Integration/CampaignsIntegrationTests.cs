using MarketingPoc.Data;
using MarketingPoc.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarketingPoc.Tests.Integration;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresPersistentContainerFixture>
{
}

[Collection("Database")]
public class CampaignsIntegrationTests
{
    private readonly PostgresPersistentContainerFixture _fixture;

    public CampaignsIntegrationTests(PostgresPersistentContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetCampaignsByTenant_ReturnsEmptyWhenNoData()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new CampaignRepository(context);
        var campaigns = await repository.GetCampaignsByTenantAsync(1, 1, 10);

        Assert.NotNull(campaigns);
        Assert.Empty(campaigns);
    }
}

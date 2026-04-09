using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;

namespace MarketingPoc.Tests.Integration;

public sealed class PostgresPersistentContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _container;
    public string ConnectionString => _container.ConnectionString;
    public string DataDirectory { get; } = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".pgdata"));

    public PostgresPersistentContainerFixture()
    {
        Directory.CreateDirectory(DataDirectory);

        _container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "marketingpoc_test",
                Username = "postgres",
                Password = "postgres"
            })
            .WithBindMount(DataDirectory, "/var/lib/postgresql/data")
            .WithPortBinding(5433, 5432)
            .WithCleanUp(false)
            .WithName("marketingpoc_test_db")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}

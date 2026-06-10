using Microsoft.Extensions.Configuration;
using MinGo.Identity.Configuration;
using Npgsql;
using Xunit;

namespace MinGo.Identity.Migrator.IntegrationTests;

/// <summary>
/// Test database lifecycle fixture.
/// Creates a clean test database before tests and drops it after.
///
/// Configuration priority (highest to lowest):
///   1. Environment variable MINGO_TEST_CONNECTION_STRING
///   2. UserSecrets (dotnet user-secrets set)
///   3. appsettings.json
///
/// Parsed config is exposed as <see cref="StorageConfiguration"/>
/// for reuse by MigratorService — ensuring a single config source.
/// </summary>
public sealed class DbFixture : IAsyncLifetime
{
    private readonly string _adminConnectionString;
    private readonly NpgsqlConnectionStringBuilder _connBuilder;
    private readonly bool _skipCleanup;

    /// <summary>Parsed configuration for reuse by test infrastructure.</summary>
    public StorageConfiguration StorageConfiguration { get; }

    /// <summary>Final connection string after applying all overrides.</summary>
    public string ConnectionString { get; }

    /// <summary>Test database name extracted from connection string.</summary>
    public string DatabaseName { get; }

    public DbFixture()
    {
        // 1. Build configuration from multiple sources
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<DbFixture>(optional: true)
            .Build();

        var storageConfig = ConfigurationHelper.LoadStorageConfiguration(config);

        // 2. Environment variable override (highest priority, manually read for clarity)
        var envOverride = Environment.GetEnvironmentVariable("MINGO_TEST_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envOverride))
            storageConfig.ConnectionString = envOverride;

        StorageConfiguration = storageConfig;
        ConnectionString = storageConfig.ConnectionString;
        _connBuilder = new NpgsqlConnectionStringBuilder(ConnectionString);
        DatabaseName = _connBuilder.Database!;

        // Admin connection uses 'postgres' default database for CREATE/DROP
        _connBuilder.Database = "postgres";
        _adminConnectionString = _connBuilder.ConnectionString;

        _skipCleanup = Environment.GetEnvironmentVariable("MINGO_TEST_SKIP_CLEANUP") == "1";
    }

    public async Task InitializeAsync()
    {
        await DropDatabaseIfExists();
        await CreateDatabase();
    }

    public async Task DisposeAsync()
    {
        if (!_skipCleanup)
            await DropDatabaseIfExists();
    }

    private async Task DropDatabaseIfExists()
    {
        await using var conn = new NpgsqlConnection(_adminConnectionString);
        await conn.OpenAsync();

        await using var terminateCmd = new NpgsqlCommand(
            """
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = @db
              AND pid <> pg_backend_pid()
            """, conn);
        terminateCmd.Parameters.AddWithValue("db", DatabaseName);
        await terminateCmd.ExecuteNonQueryAsync();

        await using var dropCmd = new NpgsqlCommand(
            $"""DROP DATABASE IF EXISTS "{DatabaseName}" """, conn);
        await dropCmd.ExecuteNonQueryAsync();
    }

    private async Task CreateDatabase()
    {
        await using var conn = new NpgsqlConnection(_adminConnectionString);
        await conn.OpenAsync();

        await using var createCmd = new NpgsqlCommand(
            $"""CREATE DATABASE "{DatabaseName}" """, conn);
        await createCmd.ExecuteNonQueryAsync();
    }
}

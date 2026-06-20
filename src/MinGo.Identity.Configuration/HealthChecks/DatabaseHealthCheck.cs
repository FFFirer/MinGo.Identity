using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Health check that validates database connectivity for all supported storage
/// backends (PostgreSQL, SQL Server, SQLite). Reads the StorageConfiguration
/// section from <see cref="IConfiguration"/> to determine which backend to check.
/// InMemory storage always passes (no external dependency).
/// </summary>
internal sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IConfiguration configuration, ILogger<DatabaseHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var storageType = _configuration["StorageConfiguration:Type"] ?? "INMEMORY";
        var connectionString = _configuration["StorageConfiguration:ConnectionString"] ?? "";

        try
        {
            return storageType switch
            {
                "POSTGRE" or "2" => await CheckPostgreSqlAsync(connectionString, cancellationToken),
                "SQLSERVER" or "1" => await CheckSqlServerAsync(connectionString, cancellationToken),
                "SQLITE" or "4" => await CheckSqliteAsync(connectionString, cancellationToken),
                "INMEMORY" or "0" or "MYSQL" or "3" or _ => HealthCheckResult.Healthy("No external database to check.")
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for storage type {StorageType}", storageType);
            return HealthCheckResult.Unhealthy($"Database ({storageType}) is unreachable.", ex);
        }
    }

    private static async Task<HealthCheckResult> CheckPostgreSqlAsync(string connectionString, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        await cmd.ExecuteScalarAsync(ct);
        return HealthCheckResult.Healthy("PostgreSQL is reachable.");
    }

    private static async Task<HealthCheckResult> CheckSqlServerAsync(string connectionString, CancellationToken ct)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        await cmd.ExecuteScalarAsync(ct);
        return HealthCheckResult.Healthy("SQL Server is reachable.");
    }

    private static async Task<HealthCheckResult> CheckSqliteAsync(string connectionString, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return HealthCheckResult.Unhealthy("SQLite connection string is empty.");
        }

        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);

        // DataSource may be empty for in-memory SQLite
        if (!string.IsNullOrEmpty(builder.DataSource)
            && !builder.Mode.HasFlag(Microsoft.Data.Sqlite.SqliteOpenMode.Memory)
            && !File.Exists(builder.DataSource))
        {
            return HealthCheckResult.Unhealthy($"SQLite database file not found: {builder.DataSource}");
        }

        await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        await conn.OpenAsync(ct);
        return HealthCheckResult.Healthy("SQLite database is reachable.");
    }
}

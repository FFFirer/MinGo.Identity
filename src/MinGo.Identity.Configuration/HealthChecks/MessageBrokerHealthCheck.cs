using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Health check for the MassTransit message broker transport.
/// Currently supports SQL Server transport and InMemory.
/// </summary>
internal sealed class MessageBrokerHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public MessageBrokerHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var transport = _configuration["MessageBrokerOptions:Transport"] ?? "INMEMORY";

        try
        {
            return transport switch
            {
                "SQLSERVER" or "1" => await CheckSqlServerTransportAsync(cancellationToken),
                "INMEMORY" or "0" or _ => HealthCheckResult.Healthy("InMemory message broker transport.")
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Message broker transport is unreachable.", ex);
        }
    }

    private async Task<HealthCheckResult> CheckSqlServerTransportAsync(CancellationToken ct)
    {
        var connectionString = _configuration["MessageBrokerOptions:ConnectionString"] ?? "";

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        await cmd.ExecuteScalarAsync(ct);

        return HealthCheckResult.Healthy("SQL Server message broker transport is reachable.");
    }
}

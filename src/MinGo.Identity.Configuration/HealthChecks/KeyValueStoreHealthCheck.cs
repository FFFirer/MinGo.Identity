using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Health check for the key-value store backend.
/// Currently supports Redis and InMemory.
/// </summary>
internal sealed class KeyValueStoreHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public KeyValueStoreHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var kvType = _configuration["KeyValueConfiguration:Type"] ?? "INMEMORY";

        try
        {
            return kvType switch
            {
                "REDIS" or "1" => await CheckRedisAsync(cancellationToken),
                "INMEMORY" or "0" or _ => HealthCheckResult.Healthy("InMemory key-value store.")
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Key-value store is unreachable.", ex);
        }
    }

    private async Task<HealthCheckResult> CheckRedisAsync(CancellationToken ct)
    {
        var connectionString = _configuration["KeyValueConfiguration:ConnectionString"] ?? "";

        await using var multiplexer = await ConnectionMultiplexer.ConnectAsync(
            ConfigurationOptions.Parse(connectionString));
        var db = multiplexer.GetDatabase();
        await db.PingAsync();

        return HealthCheckResult.Healthy("Redis key-value store is reachable.");
    }
}

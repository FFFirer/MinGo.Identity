using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Extension methods for adding identity-service health checks with
/// Kubernetes-aligned probe endpoints (/healthz, /readyz, /startupz).
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Register the base health check services.
    /// </summary>
    public static IHealthChecksBuilder AddIdentityHealthChecks(
        this IServiceCollection services)
    {
        return services.AddHealthChecks();
    }

    /// <summary>
    /// Add a readiness check that opens a connection to the PostgreSQL
    /// database specified by <paramref name="connectionString"/>.
    /// </summary>
    public static IHealthChecksBuilder AddPostgreSqlReadinessCheck(
        this IHealthChecksBuilder builder,
        string connectionString)
    {
        return builder.AddTypeActivatedHealthCheck<PostgreSqlHealthCheck>(
            "postgresql",
            HealthStatus.Unhealthy,
            ["ready", "postgresql"],
            connectionString);
    }

    /// <summary>
    /// Add a readiness check that verifies the IdServer OIDC discovery
    /// endpoint is reachable.
    /// </summary>
    public static IHealthChecksBuilder AddIdServerEndpointCheck(
        this IHealthChecksBuilder builder,
        string idserverBaseUrl)
    {
        return builder.AddTypeActivatedHealthCheck<IdServerHealthCheck>(
            "idserver-oidc",
            HealthStatus.Unhealthy,
            ["ready", "idserver"],
            idserverBaseUrl);
    }
}

// ────────────────────────────────────────────────────────────────
// Custom health check implementations
// ────────────────────────────────────────────────────────────────

/// <summary>
/// Health check that validates PostgreSQL connectivity by opening and
/// closing a raw Npgsql connection.
/// </summary>
internal sealed class PostgreSqlHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is reachable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "PostgreSQL is unreachable.",
                ex);
        }
    }
}

/// <summary>
/// Health check that verifies the IdServer OIDC discovery endpoint
/// is reachable by issuing an HTTP GET request.
/// </summary>
internal sealed class IdServerHealthCheck(string idserverBaseUrl) : IHealthCheck
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{idserverBaseUrl.TrimEnd('/')}/.well-known/openid-configuration";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("IdServer OIDC endpoint is reachable.");
            }

            return HealthCheckResult.Unhealthy(
                $"IdServer returned HTTP {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "IdServer OIDC endpoint is unreachable.",
                ex);
        }
    }
}

// ────────────────────────────────────────────────────────────────
// Helper for type-activated health check registration
// ────────────────────────────────────────────────────────────────

internal static class HealthCheckRegistrationHelper
{
    public static IHealthChecksBuilder AddTypeActivatedHealthCheck<TCheck>(
        this IHealthChecksBuilder builder,
        string name,
        HealthStatus failureStatus,
        IEnumerable<string>? tags,
        params object[] args)
        where TCheck : class, IHealthCheck
    {
        return builder.Add(new HealthCheckRegistration(
            name,
            sp => ActivatorUtilities.CreateInstance<TCheck>(sp, args),
            failureStatus,
            tags));
    }
}

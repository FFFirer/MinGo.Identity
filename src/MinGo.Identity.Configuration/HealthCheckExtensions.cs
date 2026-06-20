using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Extension methods for adding identity-service health checks with
/// Kubernetes-aligned probe endpoints (/healthz, /readyz, /startupz).
///
/// Reads application configuration to automatically register checks for
/// configured dependencies (database, key-value store, message broker).
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Register health check services and automatically add checks for
    /// configured external dependencies.
    ///
    /// IdServer (reads StorageConfiguration, KeyValueConfiguration, MessageBrokerOptions):
    ///   - Database           → DatabaseHealthCheck (tag: ready)
    ///   - Key-Value Store    → KeyValueStoreHealthCheck (tag: ready, Redis only)
    ///   - Message Broker     → MessageBrokerHealthCheck (tag: ready, external transport only)
    ///
    /// Admin (reads IdentityServerAdminConfiguration):
    ///   - IdServer OIDC      → IdServerEndpointCheck (tag: ready)
    ///   - SCIM Endpoint      → ScimEndpointCheck (tag: ready, optional)
    /// </summary>
    public static IServiceCollection AddIdentityHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddHealthChecks();

        // ── Database check (IdServer) ──────────────────────────────
        var storageType = configuration["StorageConfiguration:Type"] ?? "INMEMORY";
        if (storageType is not "INMEMORY" and not "0")
        {
            builder.AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy, ["ready"]);
        }

        // ── Key-Value store check (IdServer, Redis only) ───────────
        var kvType = configuration["KeyValueConfiguration:Type"] ?? "INMEMORY";
        if (kvType is "REDIS" or "1")
        {
            builder.AddCheck<KeyValueStoreHealthCheck>("keyvault", HealthStatus.Unhealthy, ["ready"]);
        }

        // ── Message broker check (IdServer, external transport) ────
        var brokerTransport = configuration["MessageBrokerOptions:Transport"] ?? "INMEMORY";
        if (brokerTransport is "SQLSERVER" or "1")
        {
            builder.AddCheck<MessageBrokerHealthCheck>("messagebroker", HealthStatus.Unhealthy, ["ready"]);
        }

        // ── Admin checks ───────────────────────────────────────────
        var idserverBaseUrl = configuration["IdentityServerAdminConfiguration:IdserverBaseUrl"];
        if (!string.IsNullOrEmpty(idserverBaseUrl))
        {
            builder.AddCheck<IdServerEndpointCheck>("idserver-oidc", HealthStatus.Unhealthy, ["ready"]);

            var scimBaseUrl = configuration["IdentityServerAdminConfiguration:ScimBaseUrl"];
            if (!string.IsNullOrEmpty(scimBaseUrl))
            {
                builder.AddCheck<ScimEndpointCheck>("scim", HealthStatus.Degraded, ["ready"]);
            }
        }

        return services;
    }

    /// <summary>
    /// Map Kubernetes-aligned health check endpoints.
    ///
    ///   /healthz   — Liveness: always 200 (process is alive)
    ///   /readyz    — Readiness: passes only when all dependencies tagged "ready" are healthy
    ///   /startupz  — Startup: always 200 after startup completes
    /// </summary>
    public static WebApplication MapIdentityHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => false,
            AllowCachingResponses = false,
            ResponseWriter = HealthCheckResponseWriters.WriteJson
        });
        app.MapHealthChecks("/readyz", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            AllowCachingResponses = false,
            ResponseWriter = HealthCheckResponseWriters.WriteJson
        });
        app.MapHealthChecks("/startupz", new HealthCheckOptions
        {
            Predicate = _ => false,
            AllowCachingResponses = false,
            ResponseWriter = HealthCheckResponseWriters.WriteJson
        });
        return app;
    }
}

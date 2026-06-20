using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Optional health check for the SCIM provisioning endpoint.
/// Only active when ScimBaseUrl is configured in IdentityServerAdminConfiguration.
/// Failures are reported as Degraded (not Unhealthy) since SCIM is non-critical
/// for Admin serving core functionality.
/// </summary>
internal sealed class ScimEndpointCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScimEndpointCheck> _logger;

    public ScimEndpointCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ScimEndpointCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var scimBaseUrl = _configuration["IdentityServerAdminConfiguration:ScimBaseUrl"];

        if (string.IsNullOrEmpty(scimBaseUrl))
        {
            return HealthCheckResult.Healthy("SCIM endpoint not configured.");
        }

        try
        {
            using var client = _httpClientFactory.CreateClient("health-check");
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.GetAsync(scimBaseUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Degraded(
                    $"SCIM endpoint returned HTTP {(int)response.StatusCode}.");
            }

            return HealthCheckResult.Healthy("SCIM endpoint is reachable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SCIM endpoint health check failed");
            return HealthCheckResult.Degraded("SCIM endpoint is unreachable.", ex);
        }
    }
}

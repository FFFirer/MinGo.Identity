using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Health check that verifies the IdServer OIDC discovery endpoint is reachable
/// and returns a valid JSON response. Used by the Admin service.
/// </summary>
internal sealed class IdServerEndpointCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdServerEndpointCheck> _logger;

    public IdServerEndpointCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<IdServerEndpointCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var idserverBaseUrl = _configuration["IdentityServerAdminConfiguration:IdserverBaseUrl"];
        var isRealmEnabledStr = _configuration["IdentityServerAdminConfiguration:IsRealmEnabled"];
        var isRealmEnabled = bool.TryParse(isRealmEnabledStr, out var realm) && realm;

        if (string.IsNullOrEmpty(idserverBaseUrl))
        {
            return HealthCheckResult.Unhealthy("IdServer base URL is not configured.");
        }

        try
        {
            var url = $"{idserverBaseUrl.TrimEnd('/')}{(isRealmEnabled ? "/master" : "")}/.well-known/openid-configuration";

            using var client = _httpClientFactory.CreateClient("health-check");
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"IdServer returned HTTP {(int)response.StatusCode}.");
            }

            // Validate response body is legitimate JSON
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("issuer", out _))
            {
                return HealthCheckResult.Unhealthy(
                    "IdServer response is not a valid OIDC discovery document.");
            }

            return HealthCheckResult.Healthy("IdServer OIDC endpoint is reachable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Healthy("Check cancelled (assumes healthy).");
        }
        catch (JsonException ex)
        {
            return HealthCheckResult.Unhealthy("IdServer response is not valid JSON.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IdServer OIDC endpoint health check failed");
            return HealthCheckResult.Unhealthy("IdServer OIDC endpoint is unreachable.", ex);
        }
    }
}

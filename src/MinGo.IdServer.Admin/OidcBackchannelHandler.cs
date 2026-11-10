using System.Diagnostics;

namespace MinGo.IdServer.Admin;

/// <summary>Routes only this provider's server-side requests over the configured private connection.</summary>
public sealed class OidcBackchannelHandler(
    Uri publicBaseUrl,
    Uri? backchannelBaseUrl,
    ILogger<OidcBackchannelHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var original = request.RequestUri ?? throw new InvalidOperationException("OIDC request URI is missing.");
        var publicPath = publicBaseUrl.AbsolutePath.TrimEnd('/');
        var matchesProvider = original.Scheme == publicBaseUrl.Scheme
            && original.Authority.Equals(publicBaseUrl.Authority, StringComparison.OrdinalIgnoreCase)
            && (original.AbsolutePath == publicPath || original.AbsolutePath.StartsWith(publicPath + "/", StringComparison.Ordinal));
        if (backchannelBaseUrl is not null && matchesProvider)
        {
            request.RequestUri = new UriBuilder(backchannelBaseUrl)
            {
                Path = backchannelBaseUrl.AbsolutePath.TrimEnd('/') + original.AbsolutePath[publicPath.Length..],
                Query = original.Query.TrimStart('?')
            }.Uri;
            request.Headers.Host = original.Authority;
            request.Headers.Remove("X-Forwarded-Proto");
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", original.Scheme);
            request.Headers.Remove("X-Forwarded-Host");
            request.Headers.TryAddWithoutValidation("X-Forwarded-Host", original.Authority);
        }

        var destination = request.RequestUri;
        var timer = Stopwatch.StartNew();
        var stage = "response_headers";
        HttpResponseMessage? response = null;
        var traceId = Activity.Current?.TraceId.ToString();
        // Paths identify discovery/JWKS/token/userinfo; query strings, headers and bodies are never logged.
        logger.LogInformation("OIDC backchannel started: {Method} {Host}{Path}, destination {DestinationHost}, TraceId {TraceId}",
            request.Method, original.Authority, original.AbsolutePath, destination.Authority, traceId);
        try
        {
            response = await base.SendAsync(request, cancellationToken);
            logger.LogDebug("OIDC backchannel headers: {Host}{Path}, StatusCode {StatusCode}, ElapsedMs {ElapsedMs}, CfRay {CfRay}, TraceId {TraceId}",
                original.Authority, original.AbsolutePath, (int)response.StatusCode, timer.ElapsedMilliseconds,
                response.Headers.TryGetValues("CF-Ray", out var rays) ? string.Join(",", rays) : null, traceId);
            stage = "response_body";
            // Buffer inside the diagnostic boundary: the observed 60s timeout occurred while reading chunked content.
            await response.Content.LoadIntoBufferAsync(10 * 1024 * 1024, cancellationToken);
            logger.LogInformation("OIDC backchannel completed: {Host}{Path}, StatusCode {StatusCode}, ElapsedMs {ElapsedMs}, TraceId {TraceId}",
                original.Authority, original.AbsolutePath, (int)response.StatusCode, timer.ElapsedMilliseconds, traceId);
            return response;
        }
        catch (Exception exception)
        {
            response?.Dispose();
            // Exception messages may contain token endpoint responses or query values; log types only.
            logger.LogError("OIDC backchannel failed: {Host}{Path}, destination {DestinationHost}, Stage {Stage}, ElapsedMs {ElapsedMs}, ErrorType {ErrorType}, InnerErrorType {InnerErrorType}, Cancelled {Cancelled}, TraceId {TraceId}",
                original.Authority, original.AbsolutePath, destination.Authority, stage, timer.ElapsedMilliseconds,
                exception.GetType().Name, exception.InnerException?.GetType().Name, cancellationToken.IsCancellationRequested, traceId);
            throw;
        }
        finally
        {
            request.RequestUri = original;
        }
    }
}

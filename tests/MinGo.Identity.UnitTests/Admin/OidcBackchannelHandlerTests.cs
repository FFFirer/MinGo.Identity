using System.Net;
using MinGo.IdServer.Admin;
using Xunit;

namespace MinGo.Identity.UnitTests.Admin;

public class OidcBackchannelHandlerTests
{
    [Theory]
    [InlineData("/master/.well-known/openid-configuration")]
    [InlineData("/master/jwks")]
    [InlineData("/other/token")]
    [InlineData("/master/userinfo")]
    public async Task Send_RoutesRealmEndpointsInternallyAndPreservesPublicIdentity(string path)
    {
        var logger = new RecordingLogger();
        var transport = new StubHandler(async (request, token) =>
        {
            Assert.Equal("http://idserver:5001" + path + "?secret=hidden", request.RequestUri!.AbsoluteUri);
            Assert.Equal("auth.example.test", request.Headers.Host);
            Assert.Equal("https", Assert.Single(request.Headers.GetValues("X-Forwarded-Proto")));
            Assert.Equal("auth.example.test", Assert.Single(request.Headers.GetValues("X-Forwarded-Host")));
            Assert.Equal("Bearer private-token", request.Headers.Authorization!.ToString());
            Assert.Equal("client_secret=private-secret", await request.Content!.ReadAsStringAsync(token));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"issuer\":\"https://auth.example.test/master\",\"access_token\":\"private-token\"}")
            };
        });
        using var client = CreateClient(transport, logger);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.example.test" + path + "?secret=hidden");
        request.Headers.Authorization = new("Bearer", "private-token");
        request.Content = new StringContent("client_secret=private-secret");
        using var response = await client.SendAsync(request);

        Assert.Contains("https://auth.example.test/master", await response.Content.ReadAsStringAsync());
        Assert.Equal("https://auth.example.test" + path + "?secret=hidden", request.RequestUri!.AbsoluteUri);
        Assert.Contains(logger.Messages, x => x.Contains("completed") && x.Contains("StatusCode 200"));
        Assert.DoesNotContain(logger.Messages, x => x.Contains("hidden") || x.Contains("private-token") || x.Contains("private-secret"));
    }

    [Theory]
    [InlineData("https://another.example.test/master/jwks", "https://auth.example.test")]
    [InlineData("https://auth.example.test.evil.test/master/jwks", "https://auth.example.test")]
    [InlineData("https://auth.example.test:444/master/jwks", "https://auth.example.test")]
    [InlineData("https://auth.example.test/identity-other/master/jwks", "https://auth.example.test/identity")]
    public async Task Send_DoesNotRewriteOtherProvidersOrPathPrefixes(string url, string publicBase)
    {
        using var client = CreateClient(new StubHandler((request, _) =>
        {
            Assert.Equal(url, request.RequestUri!.AbsoluteUri);
            Assert.False(request.Headers.Contains("X-Forwarded-Host"));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }), new RecordingLogger(), publicBase);
        using var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Send_MapsBasePathsWithoutDroppingRealm()
    {
        using var client = CreateClient(new StubHandler((request, _) =>
        {
            Assert.Equal("http://idserver:5001/private/master/jwks", request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }), new RecordingLogger(), "https://auth.example.test/identity/", "http://idserver:5001/private/");
        using var response = await client.GetAsync("https://auth.example.test/identity/master/jwks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Send_WithoutPrivateBaseUsesPublicTransport()
    {
        using var client = CreateClient(new StubHandler((request, _) =>
        {
            Assert.Equal("https://auth.example.test/master/jwks", request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }), new RecordingLogger(), privateBase: null);
        using var response = await client.GetAsync("https://auth.example.test/master/jwks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Send_BodyCancellationLogsStageAndDoesNotExposeExceptionMessage()
    {
        var logger = new RecordingLogger();
        using var cancellation = new CancellationTokenSource();
        using var client = CreateClient(new StubHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new CancelledContent() });
        }), logger);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetAsync("https://auth.example.test/master/.well-known/openid-configuration?secret=hidden", cancellation.Token));

        Assert.Contains(logger.Messages, x => x.Contains("failed") && x.Contains("response_body") && x.Contains("Cancelled True"));
        Assert.DoesNotContain(logger.Messages, x => x.Contains("completed") || x.Contains("hidden") || x.Contains("private-secret"));
    }

    private static HttpClient CreateClient(HttpMessageHandler transport, RecordingLogger logger,
        string publicBase = "https://auth.example.test", string? privateBase = "http://idserver:5001") =>
        new(new OidcBackchannelHandler(new Uri(publicBase), privateBase is null ? null : new Uri(privateBase), logger)
        { InnerHandler = transport });

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request, token);
    }

    private sealed class CancelledContent : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            throw new OperationCanceledException("private-secret");
    }

    private sealed class RecordingLogger : ILogger<OidcBackchannelHandler>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}

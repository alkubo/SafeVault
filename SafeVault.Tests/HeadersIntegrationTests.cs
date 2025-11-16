using System.Net;
using Xunit;

namespace SafeVault.Tests;

public class HeadersIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    public HeadersIntegrationTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions{ AllowAutoRedirect=false });
        _client.BaseAddress = new Uri("https://localhost");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Account/Login")]
    public async Task CspHeader_IsPresent(string path)
    {
        var resp = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(resp.Headers.Contains("Content-Security-Policy"));
        var csp = string.Join(";", resp.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("default-src 'self'", csp);
    }
}

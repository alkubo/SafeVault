using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SafeVault.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"sv_it_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Main"] = $"Data Source={_dbPath}"
            };
            config.AddInMemoryCollection(dict!);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
    }
}

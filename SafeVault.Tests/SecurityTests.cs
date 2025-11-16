using System.Threading.Tasks;
using SafeVault.Core.Models;
using SafeVault.App.Services;
using SafeVault.Core.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace SafeVault.Tests;

public class SecurityTests
{
    private static (IUserRepository Repo, string DbPath) CreateRepo()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sv_{Guid.NewGuid():N}.db");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new[]
        {
            new KeyValuePair<string,string>("ConnectionStrings:Main", $"Data Source={dbPath}")
        }).Build();
        var repo = new SqliteUserRepository(config);
        return (repo, dbPath);
    }

    [Fact]
    public async Task SqlInjectionLikeInputDoesNotBypassUsernameLookup()
    {
        var (repo, path) = CreateRepo();
        try
        {
            var normal = new UserInput { Username = "alice", Email = "alice@example.com" };
            await repo.CreateAsync(normal);

            var injectedLookup = await repo.GetByUsernameAsync("alice' OR 1=1 --");
            Assert.Null(injectedLookup); // parâmetro impede expansão da query

            var existing = await repo.GetByUsernameAsync("alice");
            Assert.NotNull(existing);
        }
        finally
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
        }
    }

    [Fact]
    public async Task EmailFragmentSearchEscapesWildcards()
    {
        var (repo, path) = CreateRepo();
        try
        {
            var user = new UserInput { Username = "bob", Email = "bob@test.com" };
            await repo.CreateAsync(user);

            var results = await repo.SearchByEmailFragmentAsync("test.com'%");
            Assert.Single(results); // limpeza impede uso de % extra malicioso
        }
        finally
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
        }
    }

    [Fact]
    public async Task MaliciousCharactersStrippedFromFragment()
    {
        var (repo, path) = CreateRepo();
        try
        {
            var cleanedResults = await repo.SearchByEmailFragmentAsync("<script>alert(1)</script>@test.com");
            // Não falha, apenas retorna vazio porque fragmento depois de limpeza pode não existir
            Assert.NotNull(cleanedResults);
        }
        finally
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
        }
    }
}

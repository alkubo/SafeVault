using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SafeVault.App.Services;
using SafeVault.Core.Models;
using SafeVault.Core.Services;
using Xunit;

namespace SafeVault.Tests;

public class AuthRepositoryTests
{
    private static (IUserRepository Repo, string DbPath) CreateRepo()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"sv_auth_{Guid.NewGuid():N}.db");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new[]
        {
            new KeyValuePair<string,string>("ConnectionStrings:Main", $"Data Source={dbPath}")
        }).Build();
        var repo = new SqliteUserRepository(config);
        return (repo, dbPath);
    }

    [Fact]
    public async Task ValidateCredentials_ReturnsTrue_ForCorrectPassword()
    {
        var (repo, path) = CreateRepo();
        try
        {
            await repo.CreateWithPasswordAsync(new User { Username = "u1", Email = "u1@test", Role = "user" }, "P4sswordA");
            var ok = await repo.ValidateCredentialsAsync("u1", "P4sswordA");
            Assert.True(ok);
        }
        finally { try { if (File.Exists(path)) File.Delete(path); } catch { } }
    }

    [Fact]
    public async Task ValidateCredentials_ReturnsFalse_ForWrongPassword()
    {
        var (repo, path) = CreateRepo();
        try
        {
            await repo.CreateWithPasswordAsync(new User { Username = "u2", Email = "u2@test", Role = "user" }, "P4sswordA");
            var ok = await repo.ValidateCredentialsAsync("u2", "bad");
            Assert.False(ok);
        }
        finally { try { if (File.Exists(path)) File.Delete(path); } catch { } }
    }
}

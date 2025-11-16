using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using SafeVault.Core.Models;
using SafeVault.Core.Services;

namespace SafeVault.App.Services;

public class SqliteUserRepository : IUserRepository
{
    private readonly string _connectionString;
    public SqliteUserRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Main") ?? "Data Source=safevault.db";
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute("CREATE TABLE IF NOT EXISTS Users (UserID INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT NOT NULL, Email TEXT NOT NULL);");
        conn.Execute("CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Username ON Users(Username);");
        // columns for auth
        try { conn.Execute("ALTER TABLE Users ADD COLUMN PasswordHash TEXT"); } catch { /* ignore if exists */ }
        try { conn.Execute("ALTER TABLE Users ADD COLUMN Role TEXT"); } catch { /* ignore if exists */ }

        // seed admin if not exists
        var existingAdmin = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Users WHERE Username = 'admin'");
        if (existingAdmin == 0)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("ChangeMe!123");
            conn.Execute("INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES (@u, @e, @p, @r)", new { u = "admin", e = "admin@local", p = hash, r = "admin" });
        }
    }

    private SqliteConnection Open() => new(_connectionString);

    public async Task<int> CreateAsync(UserInput user, CancellationToken ct = default)
    {
        const string sql = "INSERT INTO Users (Username, Email) VALUES (@Username, @Email); SELECT last_insert_rowid();";
        using var conn = Open();
        var id = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, user, cancellationToken: ct));
        return (int)id;
    }

    public async Task<UserInput?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = "SELECT Username, Email FROM Users WHERE Username = @Username";
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<UserInput>(new CommandDefinition(sql, new { Username = username }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<UserInput>> SearchByEmailFragmentAsync(string fragment, CancellationToken ct = default)
    {
        // Fragment sanitized: allow typical email chars only, then use parameter with wildcards
        var cleaned = new string(fragment.Where(c => char.IsLetterOrDigit(c) || "@._-".Contains(c)).ToArray());
        const string sql = "SELECT Username, Email FROM Users WHERE Email LIKE @Pattern";
        using var conn = Open();
        var rows = await conn.QueryAsync<UserInput>(new CommandDefinition(sql, new { Pattern = "%" + cleaned + "%" }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task<int> CreateWithPasswordAsync(User user, string password, CancellationToken ct = default)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        const string sql = "INSERT INTO Users (Username, Email, PasswordHash, Role) VALUES (@Username, @Email, @PasswordHash, @Role); SELECT last_insert_rowid();";
        using var conn = Open();
        var id = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, new { user.Username, user.Email, PasswordHash = hash, user.Role }, cancellationToken: ct));
        return (int)id;
    }

    public async Task<User?> GetUserForAuthAsync(string username, CancellationToken ct = default)
    {
        const string sql = "SELECT Username, Email, PasswordHash, COALESCE(Role,'user') as Role FROM Users WHERE Username = @Username";
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<User>(new CommandDefinition(sql, new { Username = username }, cancellationToken: ct));
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await GetUserForAuthAsync(username, ct);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash)) return false;
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task SetRoleAsync(string username, string role, CancellationToken ct = default)
    {
        const string sql = "UPDATE Users SET Role = @Role WHERE Username = @Username";
        using var conn = Open();
        await conn.ExecuteAsync(new CommandDefinition(sql, new { Role = role, Username = username }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<User>> ListAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT Username, Email, PasswordHash, COALESCE(Role,'user') as Role FROM Users ORDER BY Username";
        using var conn = Open();
        var rows = await conn.QueryAsync<User>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task UpdatePasswordAsync(string username, string newPassword, CancellationToken ct = default)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        const string sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = @Username";
        using var conn = Open();
        await conn.ExecuteAsync(new CommandDefinition(sql, new { PasswordHash = hash, Username = username }, cancellationToken: ct));
    }
}

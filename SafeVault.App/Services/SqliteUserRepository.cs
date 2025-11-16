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
}

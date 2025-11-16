using SafeVault.Core.Models;

namespace SafeVault.Core.Services;

public interface IUserRepository
{
    Task<int> CreateAsync(UserInput user, CancellationToken ct = default);
    Task<UserInput?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<UserInput>> SearchByEmailFragmentAsync(string fragment, CancellationToken ct = default);

    Task<int> CreateWithPasswordAsync(User user, string password, CancellationToken ct = default);
    Task<User?> GetUserForAuthAsync(string username, CancellationToken ct = default);
    Task<bool> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default);
    Task SetRoleAsync(string username, string role, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListAllAsync(CancellationToken ct = default);
    Task UpdatePasswordAsync(string username, string newPassword, CancellationToken ct = default);
}

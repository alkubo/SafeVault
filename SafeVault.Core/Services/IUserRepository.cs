using SafeVault.Core.Models;

namespace SafeVault.Core.Services;

public interface IUserRepository
{
    Task<int> CreateAsync(UserInput user, CancellationToken ct = default);
    Task<UserInput?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<UserInput>> SearchByEmailFragmentAsync(string fragment, CancellationToken ct = default);
}

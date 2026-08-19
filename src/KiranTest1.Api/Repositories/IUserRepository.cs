using KiranTest1.Api.Models;

namespace KiranTest1.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically adds the user unless another user already has the same email or user name.
    /// Returns null when such a user exists.
    /// </summary>
    Task<User?> AddIfUniqueAsync(User user, CancellationToken cancellationToken = default);
}

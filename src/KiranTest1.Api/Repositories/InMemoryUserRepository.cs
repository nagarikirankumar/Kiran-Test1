using KiranTest1.Api.Models;

namespace KiranTest1.Api.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = new();
    private readonly object _gate = new();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(FindByEmail(email));
        }
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(FindByUserName(userName));
        }
    }

    public Task<User?> AddIfUniqueAsync(User user, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (FindByEmail(user.Email) is not null || FindByUserName(user.UserName) is not null)
            {
                return Task.FromResult<User?>(null);
            }

            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            _users[user.Id] = user;
            return Task.FromResult<User?>(user);
        }
    }

    private User? FindByEmail(string email) => _users.Values.FirstOrDefault(u =>
        string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

    private User? FindByUserName(string userName) => _users.Values.FirstOrDefault(u =>
        string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));
}

using KiranTest1.Api.Dtos;
using KiranTest1.Api.Models;
using KiranTest1.Api.Repositories;

namespace KiranTest1.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public UserService(IUserRepository repository, IPasswordHasher passwordHasher, TimeProvider timeProvider)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<UserResponse> CreateUserAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userName = request.UserName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("A valid email is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(request));
        }

        if (await _repository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new UserAlreadyExistsException($"A user with email '{email}' already exists.");
        }

        if (await _repository.GetByUserNameAsync(userName, cancellationToken) is not null)
        {
            throw new UserAlreadyExistsException($"A user with user name '{userName}' already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = _timeProvider.GetUtcNow()
        };

        var created = await _repository.AddIfUniqueAsync(user, cancellationToken)
            ?? throw new UserAlreadyExistsException(
                $"A user with email '{email}' or user name '{userName}' already exists.");

        return ToResponse(created);
    }

    public async Task<UserResponse?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    private static UserResponse ToResponse(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };
}

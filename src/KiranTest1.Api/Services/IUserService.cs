using KiranTest1.Api.Dtos;

namespace KiranTest1.Api.Services;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
}

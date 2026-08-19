using KiranTest1.Api.Dtos;
using KiranTest1.Api.Models;
using KiranTest1.Api.Repositories;
using KiranTest1.Api.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace KiranTest1.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero));

    private UserService CreateSut() => new(_repository.Object, _passwordHasher.Object, _timeProvider);

    private static CreateUserRequest ValidRequest() => new()
    {
        UserName = "kiran",
        Email = "kiran@example.com",
        Password = "sup3rSecret!"
    };

    [Fact]
    public async Task CreateUserAsync_PersistsUserAndReturnsResponse()
    {
        _repository.Setup(r => r.GetByEmailAsync("kiran@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repository.Setup(r => r.GetByUserNameAsync("kiran", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash("sup3rSecret!")).Returns("hashed");
        User? added = null;
        _repository.Setup(r => r.AddIfUniqueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => added = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var response = await CreateSut().CreateUserAsync(ValidRequest());

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("kiran", response.UserName);
        Assert.Equal("kiran@example.com", response.Email);
        Assert.Equal(_timeProvider.GetUtcNow(), response.CreatedAt);
        Assert.NotNull(added);
        Assert.Equal("hashed", added!.PasswordHash);
    }

    [Fact]
    public async Task CreateUserAsync_TrimsInputAndLowercasesEmail()
    {
        _repository.Setup(r => r.GetByEmailAsync("Kiran@Example.COM", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repository.Setup(r => r.GetByUserNameAsync("kiran", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _repository.Setup(r => r.AddIfUniqueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        var response = await CreateSut().CreateUserAsync(new CreateUserRequest
        {
            UserName = "  kiran  ",
            Email = "  Kiran@Example.COM  ",
            Password = "sup3rSecret!"
        });

        Assert.Equal("kiran", response.UserName);
        Assert.Equal("kiran@example.com", response.Email);
    }

    [Fact]
    public async Task CreateUserAsync_NullRequest_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => CreateSut().CreateUserAsync(null!));
    }

    [Theory]
    [InlineData("", "kiran@example.com", "sup3rSecret!")]
    [InlineData("   ", "kiran@example.com", "sup3rSecret!")]
    [InlineData("kiran", "", "sup3rSecret!")]
    [InlineData("kiran", "not-an-email", "sup3rSecret!")]
    [InlineData("kiran", "kiran@example.com", "")]
    [InlineData("kiran", "kiran@example.com", "short")]
    public async Task CreateUserAsync_InvalidInput_ThrowsArgumentException(
        string userName, string email, string password)
    {
        var request = new CreateUserRequest { UserName = userName, Email = email, Password = password };

        await Assert.ThrowsAsync<ArgumentException>(() => CreateSut().CreateUserAsync(request));
        _repository.Verify(r => r.AddIfUniqueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsUserAlreadyExists()
    {
        _repository.Setup(r => r.GetByEmailAsync("kiran@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "kiran@example.com" });

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => CreateSut().CreateUserAsync(ValidRequest()));
        _repository.Verify(r => r.AddIfUniqueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateUserName_ThrowsUserAlreadyExists()
    {
        _repository.Setup(r => r.GetByEmailAsync("kiran@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repository.Setup(r => r.GetByUserNameAsync("kiran", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), UserName = "kiran" });

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => CreateSut().CreateUserAsync(ValidRequest()));
        _repository.Verify(r => r.AddIfUniqueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_RepositoryRejectsDuplicate_ThrowsUserAlreadyExists()
    {
        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repository.Setup(r => r.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _repository.Setup(r => r.AddIfUniqueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => CreateSut().CreateUserAsync(ValidRequest()));
    }

    [Fact]
    public async Task GetUserAsync_ExistingUser_ReturnsResponseWithoutPasswordHash()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = id,
                UserName = "kiran",
                Email = "kiran@example.com",
                PasswordHash = "hashed",
                CreatedAt = _timeProvider.GetUtcNow()
            });

        var response = await CreateSut().GetUserAsync(id);

        Assert.NotNull(response);
        Assert.Equal(id, response!.Id);
        Assert.Equal("kiran", response.UserName);
    }

    [Fact]
    public async Task GetUserAsync_MissingUser_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        Assert.Null(await CreateSut().GetUserAsync(id));
    }
}

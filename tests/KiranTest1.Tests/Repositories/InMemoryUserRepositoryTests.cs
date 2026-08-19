using KiranTest1.Api.Models;
using KiranTest1.Api.Repositories;

namespace KiranTest1.Tests.Repositories;

public class InMemoryUserRepositoryTests
{
    private readonly InMemoryUserRepository _sut = new();

    private static User NewUser() => new()
    {
        UserName = "kiran",
        Email = "kiran@example.com",
        PasswordHash = "hashed"
    };

    [Fact]
    public async Task AddIfUniqueAsync_AssignsIdWhenEmpty()
    {
        var added = await _sut.AddIfUniqueAsync(NewUser());

        Assert.NotNull(added);
        Assert.NotEqual(Guid.Empty, added!.Id);
        Assert.Equal(added.Id, (await _sut.GetByIdAsync(added.Id))!.Id);
    }

    [Fact]
    public async Task AddIfUniqueAsync_KeepsProvidedId()
    {
        var id = Guid.NewGuid();
        var user = NewUser();
        user.Id = id;

        var added = await _sut.AddIfUniqueAsync(user);

        Assert.Equal(id, added!.Id);
    }

    [Theory]
    [InlineData("other", "kiran@example.com")]
    [InlineData("kiran", "other@example.com")]
    [InlineData("KIRAN", "OTHER@EXAMPLE.COM")]
    public async Task AddIfUniqueAsync_DuplicateEmailOrUserName_ReturnsNull(string userName, string email)
    {
        await _sut.AddIfUniqueAsync(NewUser());

        var duplicate = NewUser();
        duplicate.UserName = userName;
        duplicate.Email = email;

        Assert.Null(await _sut.AddIfUniqueAsync(duplicate));
    }

    [Fact]
    public async Task AddIfUniqueAsync_ConcurrentDuplicates_AddsExactlyOne()
    {
        var attempts = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(() => _sut.AddIfUniqueAsync(NewUser())));

        var results = await Task.WhenAll(attempts);

        Assert.Single(results.Where(u => u is not null));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        await _sut.AddIfUniqueAsync(NewUser());

        Assert.NotNull(await _sut.GetByEmailAsync("KIRAN@EXAMPLE.COM"));
        Assert.Null(await _sut.GetByEmailAsync("other@example.com"));
    }

    [Fact]
    public async Task GetByUserNameAsync_IsCaseInsensitive()
    {
        await _sut.AddIfUniqueAsync(NewUser());

        Assert.NotNull(await _sut.GetByUserNameAsync("KIRAN"));
        Assert.Null(await _sut.GetByUserNameAsync("someone"));
    }
}

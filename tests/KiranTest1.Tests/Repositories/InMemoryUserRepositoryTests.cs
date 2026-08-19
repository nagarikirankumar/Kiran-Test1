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
    public async Task AddAsync_AssignsIdWhenEmpty()
    {
        var added = await _sut.AddAsync(NewUser());

        Assert.NotEqual(Guid.Empty, added.Id);
        Assert.Equal(added.Id, (await _sut.GetByIdAsync(added.Id))!.Id);
    }

    [Fact]
    public async Task AddAsync_KeepsProvidedId()
    {
        var id = Guid.NewGuid();
        var user = NewUser();
        user.Id = id;

        var added = await _sut.AddAsync(user);

        Assert.Equal(id, added.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        await _sut.AddAsync(NewUser());

        Assert.NotNull(await _sut.GetByEmailAsync("KIRAN@EXAMPLE.COM"));
        Assert.Null(await _sut.GetByEmailAsync("other@example.com"));
    }

    [Fact]
    public async Task GetByUserNameAsync_IsCaseInsensitive()
    {
        await _sut.AddAsync(NewUser());

        Assert.NotNull(await _sut.GetByUserNameAsync("KIRAN"));
        Assert.Null(await _sut.GetByUserNameAsync("someone"));
    }
}

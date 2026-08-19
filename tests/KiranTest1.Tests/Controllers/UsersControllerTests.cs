using KiranTest1.Api.Controllers;
using KiranTest1.Api.Dtos;
using KiranTest1.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KiranTest1.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userService = new();

    private UsersController CreateSut() => new(_userService.Object);

    private static CreateUserRequest ValidRequest() => new()
    {
        UserName = "kiran",
        Email = "kiran@example.com",
        Password = "sup3rSecret!"
    };

    [Fact]
    public async Task CreateUser_Success_Returns201WithLocation()
    {
        var created = new UserResponse
        {
            Id = Guid.NewGuid(),
            UserName = "kiran",
            Email = "kiran@example.com",
            CreatedAt = DateTimeOffset.UnixEpoch
        };
        _userService.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await CreateSut().CreateUser(ValidRequest(), CancellationToken.None);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UsersController.GetUser), createdAt.ActionName);
        Assert.Equal(created.Id, createdAt.RouteValues!["id"]);
        Assert.Same(created, createdAt.Value);
    }

    [Fact]
    public async Task CreateUser_DuplicateUser_Returns409()
    {
        _userService.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserAlreadyExistsException("duplicate"));

        var result = await CreateSut().CreateUser(ValidRequest(), CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("duplicate", Assert.IsType<ProblemDetails>(conflict.Value).Detail);
    }

    [Fact]
    public async Task CreateUser_InvalidRequest_Returns400()
    {
        _userService.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("bad input"));

        var result = await CreateSut().CreateUser(ValidRequest(), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("bad input", Assert.IsType<ProblemDetails>(badRequest.Value).Detail);
    }

    [Fact]
    public async Task GetUser_Existing_Returns200()
    {
        var id = Guid.NewGuid();
        var user = new UserResponse { Id = id, UserName = "kiran", Email = "kiran@example.com" };
        _userService.Setup(s => s.GetUserAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await CreateSut().GetUser(id, CancellationToken.None);

        Assert.Same(user, Assert.IsType<OkObjectResult>(result).Value);
    }

    [Fact]
    public async Task GetUser_Missing_Returns404()
    {
        _userService.Setup(s => s.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserResponse?)null);

        Assert.IsType<NotFoundResult>(await CreateSut().GetUser(Guid.NewGuid(), CancellationToken.None));
    }
}

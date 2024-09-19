using API.Controllers;
using API.Core.Services;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class UserControllerUnitTests
{
    private readonly IUserService _userService;
    private readonly UserController _controller;
    private readonly CancellationToken _ct = CancellationToken.None;

    public UserControllerUnitTests()
    {
        _userService = Substitute.For<IUserService>();
        _controller = new UserController(_userService);
    }

    [Fact]
    public async Task ShouldDelete()
    {
        //Arrange
        var login = "login";
        var deleteResult = Result.Ok();
        _userService.DeleteAsync(Arg.Any<string>(), _ct)
            .Returns(deleteResult);
        
        //Act
        var result = await _controller.Delete(login, _ct);

        //Assert
        await _userService.Received(1)
            .DeleteAsync(Arg.Is<string>(x => x == login), _ct);
        Assert.IsType<NoContentResult>(result);
    }
}
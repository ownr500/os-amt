using API.Controllers;
using API.Controllers.Dtos;
using API.Core.Models;
using API.Core.Services;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class AuthControllerUnitTests
{
    private readonly IUserService _userService;
    private const string Login = "login";
    private const string Password = "password";
    private const string AccessToken = "accessToken";
    private const string RefreshToken = "refreshToken";
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Email = "john@email.com";
    private const int Age = 30;
    
    private readonly CancellationToken _ct = CancellationToken.None;

    public AuthControllerUnitTests()
    {
        _userService = Substitute.For<IUserService>();
    }
    
    [Fact]
    public async Task ShouldSignInUser()
    {
        // Arrange
        var singInResponseModel = new TokenPairModel(AccessToken, RefreshToken);
        var signInResult = new Result<TokenPairModel>()
            .WithValue(singInResponseModel);
        _userService.SingInAsync(Arg.Any<SingInModel>(), _ct)
            .Returns(signInResult);
        var controller  = new AuthController(_userService);
        var requestDto = new SigninRequestDto(Login, Password);

        // Act
        var result = await controller.SingInAsync(requestDto, _ct);

        // Assert
        await _userService.Received(1)
            .SingInAsync(Arg.Is<SingInModel>(x => x.Login == Login && x.Password == Password), _ct);
        var expected = new SingInResponseDto(AccessToken, RefreshToken);
        Assert.Equivalent(expected, result.Value);
    }
    [Fact]
    public async Task ShouldNotSignInUser()
    {
        // Arrange
        var error = "Test failed";
        var errors = new List<string>{error};
        var signInResult = new Result<TokenPairModel>()
            .WithErrors(errors);
        _userService.SingInAsync(Arg.Any<SingInModel>(), _ct)
            .Returns(signInResult);
        var controller  = new AuthController(_userService);
        var requestDto = new SigninRequestDto(Login, Password);

        // Act
        var result = await controller.SingInAsync(requestDto, _ct);
        
        // Assert
        await _userService.Received(1)
            .SingInAsync(Arg.Is<SingInModel>(x => x.Login == Login && x.Password == Password), _ct);
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var businessError = Assert.IsType<BusinessErrorDto>(conflictResult.Value);
        Assert.Equivalent(errors, businessError.Messages);
    }
}

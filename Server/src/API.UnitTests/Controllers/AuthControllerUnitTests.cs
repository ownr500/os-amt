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
    [Fact]
    public async Task ShouldSignInUser()
    {
        // Arrange
        const string login = "login";
        const string password = "password";
        const string accessToken = "accessToken";
        const string refreshToken = "refreshToken";
        var ct = CancellationToken.None;
        var singInResponseModel = new TokenPairModel(accessToken, refreshToken);
        var signInResult = new Result<TokenPairModel>()
            .WithValue(singInResponseModel);
        var userService = Substitute
            .For<IUserService>();
        userService.SingInAsync(Arg.Any<SingInModel>(), ct)
            .Returns(signInResult);
        var controller  = new AuthController(userService);
        var requestDto = new SigninRequestDto(login, password);

        // Act
        var result = await controller.SingInAsync(requestDto, ct);

        // Assert
        await userService.Received(1)
            .SingInAsync(Arg.Is<SingInModel>(x => x.Login == login && x.Password == password), ct);
        var expected = new SingInResponseDto(accessToken, refreshToken);
        Assert.Equivalent(expected, result.Value);
    }
    [Fact]
    public async Task ShouldNotSignInUser()
    {
        // Arrange
        const string login = "login";
        const string password = "password";
        const string accessToken = "accessToken";
        const string refreshToken = "refreshToken";
        var ct = CancellationToken.None;
        var error = "Test failed";
        var errors = new List<string>{error};
        var signInResult = new Result<TokenPairModel>()
            .WithErrors(errors);
        var userService = Substitute
            .For<IUserService>();
        userService.SingInAsync(Arg.Any<SingInModel>(), ct)
            .Returns(signInResult);
        var controller  = new AuthController(userService);
        var requestDto = new SigninRequestDto(login, password);

        // Act
        var result = await controller.SingInAsync(requestDto, ct);
        
        // Assert
        await userService.Received(1)
            .SingInAsync(Arg.Is<SingInModel>(x => x.Login == login && x.Password == password), ct);
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var businessError = Assert.IsType<BusinessErrorDto>(conflictResult.Value);
        Assert.Equivalent(errors, businessError.Messages);
    }
}

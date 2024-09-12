using API.Controllers;
using API.Controllers.DTO;
using API.Models.Request;
using API.Models.Response;
using API.Services.Interfaces;
using FluentResults;
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
        var singInResponseModel = new SinginResponseModel(accessToken, refreshToken);
        var signInResult = new Result<SinginResponseModel>()
            .WithValue(singInResponseModel);
        var userService = Substitute
            .For<IUserService>();
        userService.SingInAsync(Arg.Any<SinginRequestModel>(), ct)
            .Returns(signInResult);
        var controller  = new AuthController(userService);
        var requestDto = new SigninRequestDto(login, password);

        // Act
        var result = await controller.SigninAsync(requestDto, ct);

        // Assert
        await userService.Received(1)
            .SingInAsync(Arg.Is<SinginRequestModel>(x => x.Login == login && x.Password == password), ct);
        var expected = new SinginResponseDto(accessToken, refreshToken);
        Assert.Equivalent(expected, result.Value);
    }
}

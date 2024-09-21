using API.Constants;
using API.Core.Entities;
using API.Core.Models;
using API.Core.Services;
using API.Implementation.Services;
using API.UnitTests.Helpers;
using FluentResults;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class UserServiceUnitTests
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly CancellationToken _ct = CancellationToken.None;
    
    private const string FirstUserLogin = "FirstUserLogin";
    private const string SecondUserLogin = "SecondUserLogin";

    private const string Email = "john@email.com";
    private const int Age = 30;
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Password = "12345";
    
    

    public UserServiceUnitTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        _emailService = Substitute.For<IEmailService>();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    [Fact]
    public async Task ShouldDeleteAsync()
    {
        //Arrange
        var firstUser = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
        };
        
        var secondUser = new UserEntity
        {
            LoginNormalized = SecondUserLogin.ToLower(),
        };
        
        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(firstUser);
        dbContext.Users.Add(secondUser);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.DeleteAsync(FirstUserLogin.ToUpper(), _ct);

        //Assert
        var firstUserExists = dbContext.Users.Any(x => x.LoginNormalized == firstUser.LoginNormalized);
        var secondUserExists = dbContext.Users.Any(x => x.LoginNormalized == secondUser.LoginNormalized);
        Assert.Equal(false, firstUserExists);
        Assert.Equal(true, secondUserExists);
        Assert.Equal(true, actual.IsSuccess);
    }
    
    
    [Fact]
    public async Task ShouldNotDeleteAsync()
    {
        //Arrange
        var errorMessage = MessageConstants.UserNotFound;
        var errors = new List<string> { errorMessage };
        var expectedResult = new Result().WithErrors(errors); 
        var firstUser = new UserEntity
        {
            LoginNormalized = FirstUserLogin.ToLower(),
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(firstUser);
        await dbContext.SaveChangesAsync(_ct);
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = await userService.DeleteAsync(SecondUserLogin.ToUpper(), _ct);

        //Assert
        var firstUserExists = dbContext.Users.Any(x => x.LoginNormalized == firstUser.LoginNormalized);
        var secondUserExists = dbContext.Users.Any(x => x.LoginNormalized == SecondUserLogin.ToLower());
        Assert.Equal(true, firstUserExists);
        Assert.Equal(false, secondUserExists);
        Assert.Equivalent(expectedResult, actual);
    }

    [Fact]
    public async Task ShouldRegisterAsync()
    {
        //Arrange
        var dbContext = DbHelper.CreateDbContext();
        var registerModel = new RegisterModel(FirstName, LastName, Email, Age, FirstUserLogin, Password);
        var expected = Result.Ok();
        var userService = new UserService(dbContext, _tokenService, _emailService, _contextAccessor);
        
        //Act
        var actual = userService.RegisterAsync(registerModel, _ct);
        
        //Assert
        var userCreated = dbContext.Users.Any(x => x.LoginNormalized == FirstUserLogin.ToLower());
        Assert.Equal(true, userCreated);
        Assert.Equivalent(expected, actual.Result);
    }
}
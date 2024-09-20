using API.Core.Entities;
using API.Core.Services;
using API.Implementation.Services;
using API.Infrastructure;
using API.UnitTests.Helpers;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class UserServiceUnitTests
{
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly CancellationToken _ct = CancellationToken.None;
    
    private readonly string FirstUserLogin = "FirstUserLogin";
    private readonly string SecondUserLogin = "SecondUserLogin";

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
}
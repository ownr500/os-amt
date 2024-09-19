using API.Core.Entities;
using API.Core.Services;
using API.Implementation.Services;
using API.Infrastructure;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class UserServiceUnitTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly UserService _userService;

    private readonly string FirstName = "John";
    private readonly string LastName = "Doe";
    private readonly int Age = 30;
    private readonly string Login = "login";
    private readonly string LoginNormalized = "login";
    private readonly string Email = "john@email.com";
    private readonly string EmailNormalized = "john@email.com";
    private readonly string PasswordHash = "ASUIDSADJQKJBQKQ";
    

    public UserServiceUnitTests()
    {
        _dbContext = Substitute.For<ApplicationDbContext>();
        _tokenService = Substitute.For<ITokenService>();
        _emailService = Substitute.For<IEmailService>();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _userService = new UserService(_dbContext, _tokenService, _emailService, _contextAccessor);
    }

    [Fact]
    public async Task ShouldDelete()
    {
        //Arrange
        string login = "login";
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = FirstName,
            LastName = LastName,
            Age = Age,
            Email = Email,
            EmailNormalized = EmailNormalized,
            Login = Login,
            LoginNormalized = LoginNormalized,
            PasswordHash = PasswordHash
        };


        //Act

        //Assert

    }
}
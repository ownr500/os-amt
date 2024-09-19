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
        
        //Act
        
        //Assert
        
    }
}
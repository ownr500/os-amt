using System.IdentityModel.Tokens.Jwt;
using API.Constants;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Options;
using API.Implementation.Services;
using API.UnitTests.Helpers;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class TokenServiceUnitTests
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IOptions<TokenOptions> _options;

    private const string AccessToken = "accessToken";
    private const string NewAccessToken = "newAccessToken";
    private const string RefreshToken = "refreshToken";
    private const string NewRefreshToken = "newRefreshToken";
    private const string AnotherRefreshToken = "RefreshToken1";
    private const string RecoveryToken = "recoveryToken";
    private readonly CancellationToken _ct = CancellationToken.None;
    
    public TokenServiceUnitTests()
    {
        _tokenHandler = Substitute.For<JwtSecurityTokenHandler>();
        _options = Substitute.For<IOptions<TokenOptions>>();
    }

    [Fact]
    public async Task ShouldGenerateNewTokenFromRefreshTokenAsync()
    {
        //Arrange
        var tokenPairModel = new TokenPairModel(NewAccessToken, NewRefreshToken);
        var expected = Result.Ok(tokenPairModel);
        
        _options.Value.Returns(new TokenOptions
        {
            TokenInfos = new Dictionary<TokenType, TokenInfo>
            {
                {
                    TokenType.Access, new TokenInfo
                    {
                        Audience = "Access",
                        Issuer = "localhost",
                        SecretKey = "secret",
                        LifeTimeInMinutes = 15
                    }
                },
                {
                    TokenType.Refresh, new TokenInfo
                    {
                        Audience = "Refresh",
                        Issuer = "localhost",
                        SecretKey = "secret",
                        LifeTimeInMinutes = 1440
                    }
                }
            }
        });

        _tokenHandler.WriteToken(Arg.Is<JwtSecurityToken>(x => x.Audiences.Contains("Access")))
            .Returns("newAccessToken");
        _tokenHandler.WriteToken(Arg.Is<JwtSecurityToken>(x => x.Audiences.Contains("Refresh")))
            .Returns("newRefreshToken");

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = RoleConstants.UserRoleId
                }
            }
        };
        
        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            RefreshTokenActive = true,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.False(refreshTokenActive);
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task ShouldNotGenerateNewTokenFromRefreshTokenBecauseMissingTokenAsync()
    {
        //Arrange
        var expected = Result.Fail(MessageConstants.InvalidRefreshToken);
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = new List<UserRoleEntity>
            {
                new()
                {
                    RoleId = RoleConstants.UserRoleId
                }
            }
        };
        
        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = AccessToken,
            RefreshToken = AnotherRefreshToken,
            RefreshTokenActive = true,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        _tokenHandler.Received(0).WriteToken(Arg.Any<JwtSecurityToken>());
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.True(refreshTokenActive);
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }
}
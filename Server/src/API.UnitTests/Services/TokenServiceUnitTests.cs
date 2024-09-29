using System.IdentityModel.Tokens.Jwt;
using API.Constants;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Options;
using API.Core.Services;
using API.Implementation.Services;
using API.UnitTests.Helpers;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class TokenServiceUnitTests
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IOptions<TokenOptions> _options;
    private readonly IJwtSecurityTokenProvider _tokenProvider;
    private readonly ISystemClock _systemClock;

    private const string AccessToken = "accessToken";
    private const string NewAccessToken = "newAccessToken";
    private const string RefreshToken = "refreshToken";
    private const string RecoveryToken = "recoveryToken";
    private const string NewRefreshToken = "newRefreshToken";
    private const string AnotherRefreshToken = "RefreshToken1";
    private const string AudienceAccess = "Access";
    private const string AudienceRefresh = "Refresh";
    private readonly DateTimeOffset _utcNow = DateTimeOffset.UtcNow;
    private readonly CancellationToken _ct = CancellationToken.None;

    private readonly TokenInfo _accessTokenInfo = new()
    {
        Audience = "Access",
        Issuer = "localhost",
        SecretKey = "secret",
        LifeTimeInMinutes = 15
    };
    
    private readonly TokenInfo _refreshTokenInfo = new()
    {
        Audience = "Refresh",
        Issuer = "localhost",
        SecretKey = "secret",
        LifeTimeInMinutes = 1440
    };
    
    public TokenServiceUnitTests()
    {
        _tokenHandler = Substitute.For<JwtSecurityTokenHandler>();
        _options = Substitute.For<IOptions<TokenOptions>>();
        _tokenProvider = Substitute.For<IJwtSecurityTokenProvider>();
        _systemClock = Substitute.For<ISystemClock>();
        _systemClock.UtcNow.Returns(_utcNow);
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
                    TokenType.Access, _accessTokenInfo
                },
                {
                    TokenType.Refresh, _refreshTokenInfo
                    
                }
            }
        });

        var accessOptions = new JwtSecurityToken();
        _tokenProvider
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceAccess),
                _utcNow.DateTime.AddMinutes(_accessTokenInfo.LifeTimeInMinutes)).Returns(accessOptions);
        
        var refreshOptions = new JwtSecurityToken();
        _tokenProvider.Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRefresh),
            _utcNow.DateTime.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes)).Returns(refreshOptions);

        _tokenHandler.WriteToken(accessOptions)
            .Returns(NewAccessToken);
        _tokenHandler.WriteToken(refreshOptions)
            .Returns(NewRefreshToken);

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
            RefreshTokenExpireAt = _utcNow.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

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

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

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

    [Fact]
    public async Task ShouldNotGenerateNewTokenFromRefreshTokenBecauseRefreshTokenInactiveAsync()
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
            RefreshToken = RefreshToken,
            RefreshTokenActive = false,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow.AddDays(1)
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateNewTokenFromRefreshTokenAsync(RefreshToken, _ct);

        //Assert
        _tokenHandler.Received(0).WriteToken(Arg.Any<JwtSecurityToken>());
        var refreshTokenActive = await dbContext.Tokens
            .Where(x => x.Id == token.Id)
            .Select(x => x.RefreshTokenActive)
            .FirstOrDefaultAsync(_ct);
        Assert.False(refreshTokenActive);
        Assert.Equivalent(expected.IsFailed, actual.IsFailed);
        Assert.Equivalent(expected.Errors, actual.Errors);
    }

    [Fact]
    public async Task ShouldNotGenerateNewTokenFromRefreshTokenBecauseRefreshExpiredAsync()
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
            RefreshToken = RefreshToken,
            RefreshTokenActive = true,
            RefreshTokenExpireAt = DateTimeOffset.UtcNow
        };

        var dbContext = DbHelper.CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

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

    [Theory]
    [InlineData(new[] { Role.User })]
    [InlineData(new[] { Role.Admin, Role.User })]
    public async Task ShouldGenerateTokenPairAsync(Role[] roles)
    {
        //Arrange
        var expected = new TokenPairModel(AccessToken, RefreshToken);
        
        _options.Value.Returns(new TokenOptions
        {
            TokenInfos = new Dictionary<TokenType, TokenInfo>
            {
                {
                    TokenType.Access, _accessTokenInfo
                },
                {
                    TokenType.Refresh, _refreshTokenInfo
                }
            }
        });

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserRoles = roles.Select(x => new UserRoleEntity
            {
                RoleId = RoleConstants.RoleIds[x]
            }).ToList()
        };
        var dbContext = DbHelper.CreateSqLiteDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);
        dbContext.ChangeTracker.Clear();
        
        var accessOptions = new JwtSecurityToken();
        _tokenProvider
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceAccess),
            _utcNow.DateTime.AddMinutes(_accessTokenInfo.LifeTimeInMinutes)).Returns(accessOptions);
        
        var refreshOptions = new JwtSecurityToken();
        _tokenProvider.Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRefresh),
                _utcNow.DateTime.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes)).Returns(refreshOptions);

        _tokenHandler.WriteToken(accessOptions)
            .Returns(AccessToken);
        _tokenHandler.WriteToken(refreshOptions)
            .Returns(RefreshToken);

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = await tokenService.GenerateTokenPairAsync(user.Id, roles, _ct);

        //Assert
        var tokenCreated = await dbContext.Tokens.AnyAsync(x => x.User.Id == user.Id
                                                                    && x.RefreshTokenActive
                                                                    && x.AccessToken == AccessToken
                                                                    && x.RefreshToken == RefreshToken, _ct);

        Assert.True(tokenCreated);
        Assert.Equivalent(expected, actual);

        _tokenProvider.Received(1)
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceAccess),
                _utcNow.DateTime.AddMinutes(_accessTokenInfo.LifeTimeInMinutes));
        _tokenProvider.Received(1)
            .Get(Arg.Is<GenerateTokenModel>(x => x.TokenInfo.Audience == AudienceRefresh),
                _utcNow.DateTime.AddMinutes(_refreshTokenInfo.LifeTimeInMinutes));

        _tokenHandler.Received(1)
            .WriteToken(accessOptions);
        _tokenHandler.Received(1)
            .WriteToken(refreshOptions);
    }

    [Fact]
    public async Task ShouldGenerateRecoveryToken()
    {
        //Arrange
        var expected = RecoveryToken;
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid()
        };

        var dbContext = DbHelper.CreateSqLiteDbContext();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(_ct);

        var tokenService = new TokenService(_tokenHandler, dbContext, _options, _tokenProvider, _systemClock);

        //Act
        var actual = tokenService.GenerateRecoveryToken(user.Id);

        //Assert
        Assert.Equal(expected, actual);

    }
}
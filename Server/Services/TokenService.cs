using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Constants;
using API.Models;
using API.Models.Entities;
using API.Models.enums;
using API.Models.Response;
using API.Services.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private const string SecretKey = "IOUHBEUIQWFYQKUBQKJKHJQBIASJNDLINQ";

    public TokenService(JwtSecurityTokenHandler tokenHandler, ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor)
    {
        _tokenHandler = tokenHandler;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
    }

    public string GenerateAccessToken(Guid userId, List<RoleName> roles, out DateTimeOffset expirationDate)
    {
        expirationDate = DateTime.Now.AddHours(1);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x.ToString()));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        claims.AddRange(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            "localhost",
            "API Key",
            claims: claims,
            expires: expirationDate.UtcDateTime,
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(options);
    }

    public string GenerateRefreshToken(Guid userId, out DateTimeOffset expirationDate)
    {
        expirationDate = DateTime.UtcNow.AddDays(1);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            "localhost",
            "Refresh",
            claims: claims,
            expires: expirationDate.UtcDateTime,
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(options);
    }

    public async Task<Result<TokenModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct)
    {
        // var tokenEntity = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.RefreshToken == token, cancellationToken: ct);
        var model = await _dbContext.Tokens
            .Where(x => x.RefreshToken == token && x.RefreshTokenActive && x.RefreshTokenExpireAt > DateTimeOffset.UtcNow)
            .Select(x =>
                new
                {
                    userId = x.User.Id,
                    token = x,
                    roles = x.User.UserRoles
                        .Select(u => u.Role.RoleName)
                        .ToList()
                }
            )
            .FirstOrDefaultAsync(ct);
        if (model is not null)
        {
            model.token.RefreshTokenActive = false;
            var tokenModel = await GenerateNewTokenModelAsync(model.userId, model.roles, ct);
            _dbContext.Tokens.Update(model.token);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Ok(tokenModel);
        }

        return Result.Fail(MessageConstants.InvalidRefreshToken);
    }

    public async Task AddTokenAsync(string token)
    {
        var context = _contextAccessor.HttpContext;
        if (context is null) return;

        var newToken = new TokenEntity
        {
            Id = Guid.NewGuid(),
            AccessToken = token
        };
        await _dbContext.Tokens.AddAsync(newToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<TokenModel> GenerateNewTokenModelAsync(Guid userId, List<RoleName> roles, CancellationToken ct)
    {
        var accessToken = GenerateAccessToken(userId, roles, out var accessTokenExpireAt);
        var refreshToken = GenerateRefreshToken(userId, out var refreshTokenExpireAt);

        var token = new TokenEntity
        {
            Id = Guid.NewGuid(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = userId,
            AccessTokenExpireAt = accessTokenExpireAt,
            RefreshTokenExpireAt = refreshTokenExpireAt,
            RefreshTokenActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.AddAsync(token, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new TokenModel(accessToken, refreshToken);
    }

    public async Task<bool> CheckActiveToken(StringValues header)
    {
        var headerArray = header.ToString().Split(' ');
        if (headerArray.Length == 2)
        {
            var token = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.AccessToken == headerArray[1]);
            if (token is not null && token.RefreshTokenActive) return true;
        }
        return false;
    }

    public async Task<bool> CheckRevokedToken(StringValues header)
    {
        var headerArray = header.ToString().Split(' ');
        if (headerArray.Length == 2)
        {
            var tokenRevoked = await _dbContext.RevokedTokens.AnyAsync(x => x.Token == headerArray[1]);
            if (tokenRevoked) return false;
        }

        return true;
    }
    
    public async Task<Result> RevokeTokens(Guid? nullableUserId, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var revokedTokensCombined = await _dbContext.Tokens
            .Where(x => x.UserId == userId && x.RefreshTokenActive)
            .Select(x => new CombinedTokenModel
                {
                    Entity1 = new RevokedTokenEntity
                    {
                        Id = Guid.NewGuid(),
                        Token = x.AccessToken,
                        TokenExpireAt = x.AccessTokenExpireAt
                    },
                    Entity2 = new RevokedTokenEntity
                    {
                        Id = Guid.NewGuid(),
                        Token = x.RefreshToken,
                        TokenExpireAt = x.RefreshTokenExpireAt
                    }
                }
            )
            .ToListAsync(ct);

        var revokedTokensToAdd = revokedTokensCombined
            .SelectMany(x => new[]
            {
                x.Entity1,
                x.Entity2
            }).ToList();
        _dbContext.RevokedTokens.AddRange(revokedTokensToAdd);
        await _dbContext.Tokens
            .Where(x => x.UserId == userId && x.RefreshTokenActive)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.RefreshTokenActive, false), ct);

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result.Ok();
    }
    
    private Guid GetUserIdFromContext()
    {
        var nameIdentifier = _contextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(
                x => string.Equals(x.Type, ClaimTypes.NameIdentifier,
                    StringComparison.InvariantCultureIgnoreCase))?.Value;
        if (!Guid.TryParse(nameIdentifier, out var userId))
        {
            throw new ArgumentNullException(nameof(ClaimTypes.NameIdentifier));
        }

        return userId;
    }
}
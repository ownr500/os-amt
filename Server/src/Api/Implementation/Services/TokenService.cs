using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Constants;
using API.Core.Entities;
using API.Core.Enums;
using API.Core.Models;
using API.Core.Options;
using API.Core.Services;
using API.Extensions;
using API.Infrastructure;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace API.Implementation.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ApplicationDbContext _dbContext;
    private readonly TokenOptions _options;

    public TokenService(
        JwtSecurityTokenHandler tokenHandler,
        ApplicationDbContext dbContext,
        IOptions<TokenOptions> options
    )
    {
        _tokenHandler = tokenHandler;
        _dbContext = dbContext;
        _options = options.Value;
    }


    public async Task<Result<TokenPairModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct)
    {
        var model = await _dbContext.Tokens
            .Where(x => x.RefreshToken == token && x.RefreshTokenActive && x.RefreshTokenExpireAt > DateTimeOffset.UtcNow)
            .Select(x =>
                new
                {
                    userId = x.User.Id,
                    token = x,
                    roles = x.User.UserRoles
                        .Select(u => u.Role.Role)
                        .ToList()
                }
            )
            .FirstOrDefaultAsync(ct);
        if (model is not null)
        {
            model.token.RefreshTokenActive = false;
            
            var tokenModel = await GenerateTokenPairAsync(model.userId, model.roles, ct);
            _dbContext.Tokens.Update(model.token);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Ok(tokenModel);
        }

        return Result.Fail(MessageConstants.InvalidRefreshToken);
    }

    public async Task<TokenPairModel> GenerateTokenPairAsync(Guid userId, IReadOnlyCollection<Role> roleNames, CancellationToken ct)
    {
        var accessToken = GenerateToken(userId, roleNames, TokenType.Access);
        var refreshToken = GenerateToken(userId, Array.Empty<Role>(), TokenType.Refresh);

        var token = new TokenEntity
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            UserId = userId,
            AccessTokenExpireAt = accessToken.ExpireAt,
            RefreshTokenExpireAt = refreshToken.ExpireAt,
            RefreshTokenActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.AddAsync(token, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new TokenPairModel(accessToken.Token, refreshToken.Token);
    }

    public string GenerateRecoveryToken(Guid userId)
    {
        var token = GenerateToken(userId, Array.Empty<Role>(), TokenType.Recovery);
        
        return token.Token;
    }

    public async Task<bool> ValidateAuthHeader(StringValues header)
    {
        var headerArray = header.ToString().Split(' ');
        if (headerArray.Length == 2)
        {
            var tokenRevoked = await _dbContext.RevokedTokens.AnyAsync(x => x.Token == headerArray[1]);
            if (tokenRevoked) return false;
        }

        return true;
    }
    
    public async Task RevokeTokensAsync(Guid userId, CancellationToken ct)
    {
        var revokedTokens = await _dbContext.Tokens
            .Where(x => x.UserId == userId && x.RefreshTokenActive)
            .Select(x => new RevokedTokenEntity[]
                {
                    new()
                    {
                        Token = x.AccessToken,
                        TokenExpireAt = x.AccessTokenExpireAt
                    },
                    new()
                    {
                        Token = x.RefreshToken,
                        TokenExpireAt = x.RefreshTokenExpireAt
                    }
                }
            )
            .ToListAsync(ct);

        if (revokedTokens.Count > 0)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            _dbContext.RevokedTokens.AddRange(revokedTokens.SelectMany(x => x));
            await _dbContext.Tokens
                .Where(x => x.UserId == userId && x.RefreshTokenActive)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.RefreshTokenActive, false), ct);

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
    }
    
    private GeneratedTokenModel GenerateToken(Guid userId, IReadOnlyCollection<Role> roles, TokenType type)
    {
        var accessTokenInfo = _options.TokenInfos.GetValueOrDefault(type);
        if (accessTokenInfo is null) throw new ArgumentNullException(nameof(_options.TokenInfos));
        
        var claims = GetClaims(userId, roles);
        var generateTokenModel = accessTokenInfo.ToModel(claims);
        var accessToken = GenerateToken(generateTokenModel);

        return accessToken;
    }

    private GeneratedTokenModel GenerateToken(GenerateTokenModel model)
    {
        var expireAt = DateTimeOffset.UtcNow.AddMinutes(model.TokenInfo.LifeTimeInMinutes);
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(model.TokenInfo.SecretKey));
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            model.TokenInfo.Issuer,
            model.TokenInfo.Audience,
            claims: model.Claims,
            expires: expireAt.DateTime,
            signingCredentials: credentials
        );
        
        return new GeneratedTokenModel(_tokenHandler.WriteToken(options), expireAt);
    }

    private static List<Claim> GetClaims(Guid userId, IReadOnlyCollection<Role> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        claims.AddRange(roles.ToClaims());
        return claims;
    }
}
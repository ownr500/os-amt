using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Constants;
using API.Extensions;
using API.Models;
using API.Models.Entities;
using API.Models.enums;
using API.Models.Response;
using API.Options;
using API.Services.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TokenOptions _options;
    private const string SecretKey = "IOUHBEUIQWFYQKUBQKJKHJQBIASJNDLINQ";

    public TokenService(JwtSecurityTokenHandler tokenHandler, ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor, IOptions<TokenOptions> options)
    {
        _tokenHandler = tokenHandler;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _options = options.Value;
    }


    private string GenerateToken(GenerateTokenModel model)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(model.SecretKey));
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            model.Issuer,
            model.Issuer,
            claims: model.Claims,
            expires: model.ExpireAt,
            signingCredentials: credentials
        );
        
        return _tokenHandler.WriteToken(options);
    }
    
    public async Task<Result<TokenModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct)
    {
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
            
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, model.userId.ToString()) };
            claims.AddRange(model.roles
                .Select(x => new Claim(ClaimTypes.Role, x.ToString())).ToList());
            
            var tokenModel = await GenerateNewTokenModelAsync(model.userId, claims, ct);
            _dbContext.Tokens.Update(model.token);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Ok(tokenModel);
        }

        return Result.Fail(MessageConstants.InvalidRefreshToken);
    }

    public async Task<TokenModel> GenerateNewTokenModelAsync(Guid userId, List<Claim> claims, CancellationToken ct)
    {
        var accessTokenInfo = _options.TokenInfos.GetValueOrDefault(TokenType.Access);
        var refreshTokenInfo = _options.TokenInfos.GetValueOrDefault(TokenType.Refresh);

        if (accessTokenInfo is null || refreshTokenInfo is null) throw new ArgumentNullException();

        var accessTokenExpireAt = DateTime.UtcNow.AddMinutes(accessTokenInfo.LifeTimeInMinutes);
        var refreshTokenExpireAt = DateTime.UtcNow.AddMinutes(refreshTokenInfo.LifeTimeInMinutes);
        var generateAccessTokenModel = accessTokenInfo.ToModel(userId, claims, accessTokenExpireAt);
        var generateRefreshTokenModel = refreshTokenInfo.ToModel(userId, claims, refreshTokenExpireAt);

        var accessToken = GenerateToken(generateAccessTokenModel);
        var refreshToken = GenerateToken(generateRefreshTokenModel);
        
        var token = new TokenEntity
        {
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
    
    public async Task RevokeTokens(Guid userId, CancellationToken ct)
    {
        var revokedTokens = await _dbContext.Tokens
            .Where(x => x.UserId == userId && x.RefreshTokenActive)
            .Select(x => new RevokedTokenEntity[]
                {
                    new RevokedTokenEntity
                    {
                        Token = x.AccessToken,
                        TokenExpireAt = x.AccessTokenExpireAt
                    },
                    new RevokedTokenEntity
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

            _dbContext.RevokedTokens.AddRange(revokedTokens
                .SelectMany(x => x));
            await _dbContext.Tokens
                .Where(x => x.UserId == userId && x.RefreshTokenActive)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.RefreshTokenActive, false), ct);

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
    }
}
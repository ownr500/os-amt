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

    public string GenerateAccessToken(Guid userId, List<UserRoleEntity> roles)
    {
        var adminRoleFound = roles.FirstOrDefault(x => x.Role.RoleName == RoleName.Admin);

        var role = adminRoleFound is null ? RoleName.User.ToString() : RoleName.Admin.ToString();
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            "localhost",
            "API Key",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(options);
    }

    public string GenerateRefreshToken(Guid userId, List<UserRoleEntity> roles, out DateTimeOffset expirationDate)
    {
        expirationDate = DateTime.UtcNow.AddDays(1);
        
        var adminRoleFound = roles.FirstOrDefault(x => x.Role.RoleName == RoleName.Admin);
        var role = adminRoleFound is null ? RoleName.User.ToString() : RoleName.Admin.ToString();
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
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
        var tokenEntity = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.RefreshToken == token, cancellationToken: ct);
        if (tokenEntity is not null && tokenEntity.IsActive && tokenEntity.RefreshTokenExpireAt > DateTimeOffset.UtcNow)
        {
            tokenEntity.IsActive = false;
            var tokenModel = await GenerateNewTokenModelAsync(tokenEntity.UserId, ct);
            _dbContext.Tokens.Update(tokenEntity);
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

    public async Task<TokenModel> GenerateNewTokenModelAsync(Guid userId, CancellationToken ct)
    {
        var user = await _dbContext.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Id == userId, ct);
        
        var accessToken = GenerateAccessToken(userId, user.UserRoles.ToList());
        var refreshToken = GenerateRefreshToken(userId, user.UserRoles.ToList(), out var expirationDate);

        var token =  new TokenEntity
        {
            Id = Guid.NewGuid(),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = userId,
            RefreshTokenExpireAt = expirationDate,
            IsActive = true,
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
            if (token is not null && token.IsActive) return true;
        }
        
        return false;
    }
}
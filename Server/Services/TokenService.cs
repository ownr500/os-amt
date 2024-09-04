using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Constants;
using API.Models;
using API.Models.Entities;
using API.Models.Response;
using API.Services.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string secretKey = "IOUHBEUIQWFYQKUBQKJKHJQBIASJNDLINQ";

    public TokenService(JwtSecurityTokenHandler tokenHandler, ApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor)
    {
        _tokenHandler = tokenHandler;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
    }

    public string GenerateAccessToken(UserEntity user)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secretKey));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            "localhost",
            "API Key",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(options);
    }

    public string GenerateRefreshToken(UserEntity user)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secretKey));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var options = new JwtSecurityToken(
            "localhost",
            "Refresh",
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(options);
    }

    public async Task<Result<SinginReponseModel>> GenerateNewTokenFromRefreshToken(string token)
    {
        var tokenEntity = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.RefreshToken == token);
        if (tokenEntity is not null && tokenEntity.IsActive && tokenEntity.RefreshTokenExpireAt < DateTimeOffset.Now)
        {
            tokenEntity.IsActive = false;
            var accessToken = GenerateAccessToken(tokenEntity.User);
            var refreshToken = GenerateRefreshToken(tokenEntity.User);

            var newToken = GenerateNewTokenEntity(tokenEntity.User);
            _dbContext.Tokens.Update(tokenEntity);
            await _dbContext.Tokens.AddAsync(newToken);
            await _dbContext.SaveChangesAsync();
            return Result.Ok(new SinginReponseModel(accessToken, refreshToken));
        }

        return Result.Fail(MessageConstants.InvalidRefreshToken);
    }


    public async Task<ClaimsPrincipal> ValidateToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        };
        SecurityToken validatedToken;
        var principal = _tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        return principal;
    }
    
    public DateTimeOffset GetTokenExpiration(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidIssuer = "localhost",
            ValidAudience = "Refresh"
        };
        SecurityToken validatedToken;
        _tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        return validatedToken.ValidTo;
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

    public TokenEntity GenerateNewTokenEntity(UserEntity user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        return new TokenEntity
        {
            Id = new Guid(),
            User = user,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            RefreshTokenExpireAt = GetTokenExpiration(refreshToken),
            IsActive = true,
            CreatedAt = DateTimeOffset.Now
        };
    }
}
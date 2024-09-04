using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models;
using API.Models.Entities;
using API.Services.Interfaces;
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

    public string GenerateAuthToken(UserEntity user)
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
}
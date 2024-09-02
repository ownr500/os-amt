using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models;
using API.Models.Entitites;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ApplicationDbContext _dbContext;
    private readonly string secretKey = "IOUHBEUIQWFYQKUBQKJKHJQBIASJNDLINQ";

    public TokenService(JwtSecurityTokenHandler tokenHandler, ApplicationDbContext dbContext)
    {
        _tokenHandler = tokenHandler;
        _dbContext = dbContext;
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
        var key = Encoding.ASCII.GetBytes(secretKey);

        try
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
        catch (SecurityTokenExpiredException)
        {
            var tokenFromDb = await _dbContext.Tokens.FirstAsync(x => x.JwtToken == token);
            _dbContext.Tokens.Remove(tokenFromDb);
            await _dbContext.SaveChangesAsync();
            
            throw new SecurityTokenException("Token has expired.");
        }
        catch (Exception e)
        {
            throw new SecurityTokenException("Invalid token", e);
        }
    }

    public async Task SaveToken(string token)
    {
        var newToken = new TokenEntity
        {
            Id = Guid.NewGuid(),
            JwtToken = token
        };
        _dbContext.Tokens.Add(newToken);
        await _dbContext.SaveChangesAsync();
    }
}
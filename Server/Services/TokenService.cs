using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Models.Entitites;
using API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly string secretKey = "IOUHBEUIQWFYQKUBQKJKHJQBIASJNDLINQ";

    public TokenService(JwtSecurityTokenHandler tokenHandler)
    {
        _tokenHandler = tokenHandler;
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

    public ClaimsPrincipal ValidateToken(string token)
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
        catch (Exception e)
        {
            throw new SecurityTokenException("Invalid token", e);
        }
    }
}
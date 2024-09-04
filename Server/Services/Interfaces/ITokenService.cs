using System.Security.Claims;
using API.Models.Entities;

namespace API.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserEntity user);
    string GenerateRefreshToken(UserEntity user);
    Task<ClaimsPrincipal> ValidateToken(string token);
    Task AddTokenAsync(string token);
    DateTimeOffset GetTokenExpiration(string token);
}
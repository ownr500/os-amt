using System.Security.Claims;
using API.Models.Entities;
using FluentResults;

namespace API.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserEntity user);
    string GenerateRefreshToken(UserEntity user);
    Task<Result> GenerateNewToken(string token);
    Task<ClaimsPrincipal> ValidateToken(string token);
    Task AddTokenAsync(string token);
    DateTimeOffset GetTokenExpiration(string token);
    TokenEntity GenerateNewTokenEntity(UserEntity user);
}
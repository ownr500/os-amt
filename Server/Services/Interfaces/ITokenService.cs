using System.Security.Claims;
using API.Models.Entities;
using API.Models.Response;
using FluentResults;

namespace API.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserEntity user);
    string GenerateRefreshToken(UserEntity user);
    Task<ClaimsPrincipal> ValidateToken(string token);
    Task AddTokenAsync(string token);
    DateTimeOffset GetTokenExpiration(string token);
    Task<Result<SinginReponseModel>> GenerateNewTokenFromRefreshToken(string token);
    TokenEntity GenerateNewTokenEntity(UserEntity user);
}
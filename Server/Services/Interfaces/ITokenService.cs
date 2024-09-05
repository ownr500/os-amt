using System.Security.Claims;
using API.Models.Entities;
using API.Models.Response;
using FluentResults;

namespace API.Services.Interfaces;

public interface ITokenService
{
    Task<Result<TokenModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct);
    Task<TokenModel> GenerateNewTokenModelAsync(Guid userId, CancellationToken ct);
    Task<bool> CheckActiveToken(string header);
}
using System.Security.Claims;
using API.Models.enums;
using API.Models.Response;
using FluentResults;
using Microsoft.Extensions.Primitives;

namespace API.Services.Interfaces;

public interface ITokenService
{
    Task<Result<TokenModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct);
    Task<TokenModel> GenerateNewTokenModelAsync(Guid userId, List<Claim> claims, CancellationToken ct);
    Task<bool> CheckRevokedToken(StringValues header);
    Task RevokeTokens(Guid userId, CancellationToken ct);
}
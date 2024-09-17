using API.Models.enums;
using API.Models.Response;
using FluentResults;
using Microsoft.Extensions.Primitives;

namespace API.Services.Interfaces;

public interface ITokenService
{
    Task<Result<TokenPairModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct);
    Task<TokenPairModel> GenerateTokenPairAsync(Guid userId, List<Role> claims, CancellationToken ct);
    Task<bool> ValidateAuthHeader(StringValues header);
    Task RevokeTokensAsync(Guid userId, CancellationToken ct);
}
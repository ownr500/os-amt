using API.Core.Enums;
using API.Core.Models;
using FluentResults;
using Microsoft.Extensions.Primitives;

namespace API.Core.Services;

public interface ITokenService
{
    Task<Result<TokenPairModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct);
    Task<TokenPairModel> GenerateTokenPairAsync(Guid userId, IReadOnlyCollection<Role> claims, CancellationToken ct);
    Task<bool> ValidateAuthHeader(StringValues header);
    Task RevokeTokensAsync(Guid userId, CancellationToken ct);
    string GenerateRecoveryToken(Guid userId);
    Task<Result<RecoveryTokenModel>> ValidateRecoveryTokenAsync(string token);
    Task AddRecoveryTokenAsync(string token, DateTime valueExpireAt);
}
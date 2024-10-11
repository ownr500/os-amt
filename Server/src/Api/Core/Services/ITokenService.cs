using API.Core.Enums;
using API.Core.Models;
using FluentResults;

namespace API.Core.Services;

public interface ITokenService
{
    Task<Result<TokenPairModel>> GenerateNewTokenFromRefreshTokenAsync(string token, CancellationToken ct);
    Task<TokenPairModel> GenerateTokenPairAsync(Guid userId, IReadOnlyCollection<Role> claims, CancellationToken ct);
    Task RevokeTokensAsync(CancellationToken ct);
    string GenerateRecoveryToken(Guid userId);
    Result<UserIdAndExpireModel> ValidateRecoveryToken(string token, CancellationToken ct);
    Task AddRecoveryTokenAsync(string token, DateTimeOffset valueExpireAt, CancellationToken ct);
    Task<Result> CheckRecoveryTokenExists(string token, CancellationToken ct);
    Task RemoveExpiredTokensAsync(CancellationToken ct);
    Task RevokeTokenAsync(string accessToken, CancellationToken ct);
    Task<bool> IsCurrentTokenRevoked();
}
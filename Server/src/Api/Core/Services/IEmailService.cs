using FluentResults;

namespace API.Core.Services;

public interface IEmailService
{
    Task<Result> SendRecoveryLink(Guid userId, string email, CancellationToken ct);
}
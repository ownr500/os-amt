using FluentResults;

namespace API.Services.Interfaces;

public interface IEmailService
{
    Task<Result> SendRecoveryLink(Guid userId, string email, CancellationToken ct);
}
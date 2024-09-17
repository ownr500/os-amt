using API.Services.Interfaces;
using FluentResults;

namespace API.Services;

public class EmailService : IEmailService
{
    public Task<Result> SendRecoveryLink(Guid userId, string email, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
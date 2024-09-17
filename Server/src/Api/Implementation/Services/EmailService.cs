using API.Core.Services;
using FluentResults;

namespace API.Implementation.Services;

public class EmailService : IEmailService
{
    public Task<Result> SendRecoveryLink(Guid userId, string email, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
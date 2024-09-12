using Microsoft.AspNetCore.Mvc;

namespace API.Services.Interfaces;

public interface IEmailService
{
    Task<IActionResult> SendRecoveryLink(Guid userId, string email, CancellationToken ct);
}
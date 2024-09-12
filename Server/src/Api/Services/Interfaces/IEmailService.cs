using Microsoft.AspNetCore.Mvc;

namespace API.Services.Interfaces;

public interface IEmailService
{
    Task<IActionResult> SendRecoveryLink(string email, CancellationToken ct);
}
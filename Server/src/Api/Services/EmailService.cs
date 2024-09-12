using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Services;

public class EmailService : IEmailService
{
    public Task<IActionResult> SendRecoveryLink(Guid userId, string email, CancellationToken ct)
    {
        
        throw new NotImplementedException();
    }
}
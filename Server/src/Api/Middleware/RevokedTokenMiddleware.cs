using API.Core.Services;
using Microsoft.Net.Http.Headers;

namespace API.Middleware;

internal sealed class RevokedTokenMiddleware : IMiddleware
{
    private readonly ITokenService _tokenService;

    public RevokedTokenMiddleware(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var revoked = await _tokenService.IsCurrentTokenRevoked();
            if (revoked)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        
        await next(context);
    }
}
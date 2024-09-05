using API.Services.Interfaces;
using Microsoft.Extensions.Primitives;
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
        if(context.Request.Headers
            .TryGetValue(HeaderNames.Authorization, out var header))
        {
            var headerArray = header.ToString().Split(' ');
            if (headerArray.Length == 2)
            {
                var result = await _tokenService.CheckActiveToken(headerArray[1]);
                if (!result)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
        }
        
        await next(context);
    }
}
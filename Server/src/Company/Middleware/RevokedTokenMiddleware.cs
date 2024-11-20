using AuthContractMessages.Tokens;
using MassTransit;

namespace Company.Middleware;

internal sealed class RevokedTokenMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, IRequestClient<IsTokenRevokedRequest> client, RequestDelegate next)
    {
        
        throw new NotImplementedException();
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        
        throw new NotImplementedException();
    }
}
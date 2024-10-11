using System.Security.Claims;
using API.Core.Services;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace API.Implementation.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpContextService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
    public string GetToken()
    {
        var context = CheckContextAndThrowException();
        if (context.Request.Headers
            .TryGetValue(HeaderNames.Authorization, out var header))
        {
            return ExtractTokenFromHeader(header);
        }

        throw new ArgumentNullException(nameof(GetToken));
    }

    public Guid GetUserIdFromContext()
    {
        var nameIdentifier = _contextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(
                x => string.Equals(x.Type, ClaimTypes.NameIdentifier,
                    StringComparison.InvariantCultureIgnoreCase))?.Value;
        if (!Guid.TryParse(nameIdentifier, out var userId))
        {
            throw new ArgumentNullException(nameof(ClaimTypes.NameIdentifier));
        }

        return userId;
    }

    private static string ExtractTokenFromHeader(StringValues header)
    {
        var headerArray = header.ToString().Split(' ');
        if (headerArray.Length != 2) throw new ArgumentNullException(nameof(header));
        return headerArray[1];
    }

    private HttpContext CheckContextAndThrowException()
    {
        return _contextAccessor.HttpContext is null
            ? throw new ArgumentNullException(nameof(_contextAccessor.HttpContext))
            : _contextAccessor.HttpContext;
    }
}
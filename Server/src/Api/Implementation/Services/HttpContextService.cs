using System.Security.Claims;
using API.Core.Services;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace API.Implementation.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly HttpContext _context;

    public HttpContextService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
        _context = _contextAccessor.HttpContext is null
            ? throw new ArgumentNullException(nameof(_contextAccessor.HttpContext))
            : _contextAccessor.HttpContext;

    }
    public string GetToken()
    {
        if (_context.Request.Headers
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
}
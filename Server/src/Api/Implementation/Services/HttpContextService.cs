using System.Security.Claims;
using API.Core.Services;

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
        
        throw new NotImplementedException();
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
}
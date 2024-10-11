namespace API.Core.Services;

public interface IHttpContextService
{
    string GetToken();
    Guid GetUserIdFromContext();
}
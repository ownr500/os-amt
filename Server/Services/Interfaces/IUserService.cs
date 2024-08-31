using API.Models.Request;
using API.Models.Response;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<DeleteResponse> DeleteAsync(string login);
    Task<ChangeResponse> ChangeAsync(ChangeRequest toRequest);
}
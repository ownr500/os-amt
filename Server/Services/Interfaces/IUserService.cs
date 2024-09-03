using API.Models.Entitites;
using API.Models.Request;
using API.Models.Response;
using FluentResults;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<Result> DeleteAsync(string login, CancellationToken ct);
    Task<ChangeResponse> ChangeAsync(ChangeRequest toRequest);
    Task<bool> PasswordChangeAsync(PasswordChangeModel model);
    Task<Result<SinginReponseModel>> SinginAsync(SinginRequestModel toRequestModel);
}
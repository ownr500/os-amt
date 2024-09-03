using API.Models.Entitites;
using API.Models.Request;
using API.Models.Response;
using FluentResults;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result> DeleteAsync(string login, CancellationToken ct);
    Task<Result> ChangeAsync(ChangeRequest сhangeRequest);
    Task<Result> PasswordChangeAsync(PasswordChangeModel model);
    Task<Result<SinginReponseModel>> SinginAsync(SinginRequestModel requestModel, CancellationToken ct);
}
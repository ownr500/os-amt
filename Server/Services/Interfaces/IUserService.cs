using API.Models;
using API.Models.enums;
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
    Task<Result<SinginReponseModel>> SingInAsync(SinginRequestModel requestModel, CancellationToken ct);
    Task<List<UserModel>> GetUsers(CancellationToken ct);
    Task<Result> MakeUserAdmin(Guid userId, CancellationToken ct);
    Task<Result> RevokeTokens(Guid userId, CancellationToken ct);
    Task<Result> AddRoleAsync(Guid userId, RoleName role, CancellationToken ct);
    Task<Result> RemoveRoleAsync(Guid userId, RoleName role, CancellationToken ct);
}
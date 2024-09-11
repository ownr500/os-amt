using API.Models;
using API.Models.enums;
using API.Models.Request;
using API.Models.Response;
using FluentResults;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(string login, CancellationToken ct);
    Task<Result> ChangeAsync(ChangeRequest changeRequest, CancellationToken ct);
    Task<Result> PasswordChangeAsync(PasswordChangeModel model, CancellationToken ct);
    Task<Result<SinginReponseModel>> SingInAsync(SinginRequestModel requestModel, CancellationToken ct);
    Task<List<UserModel>> GetUsers(CancellationToken ct);
    Task<Result> AddRoleAsync(Guid userId, RoleNames role, CancellationToken ct);
    Task<Result> RemoveRoleAsync(Guid userId, RoleNames role, CancellationToken ct);
    Guid GetUserIdFromContext();
}
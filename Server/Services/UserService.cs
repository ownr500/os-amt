using System.Security.Claims;
using API.Models.Request;
using API.Models.Response;
using API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using API.Constants;
using API.Extensions;
using API.Models;
using API.Models.Entities;
using API.Models.enums;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(ApplicationDbContext dbContext, ITokenService tokenService, IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var userExists = _dbContext.Users.Any(x => x.LoginNormalized == request.Login.ToLower());
        if (userExists) return Result.Fail(MessageConstants.UserAlreadyRegistered);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Age = request.Age,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Login = request.Login,
            LoginNormalized = request.Login.ToLower(),
            PasswordHash = GeneratePasswordHash(request.Password)
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(string login, CancellationToken ct)
    {
        var user = await GetUserByLoginAsync(login, ct);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ChangeAsync(ChangeRequest changeRequest)
    {
        var userId = GetUserIdFromContext();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        user.FirstName = changeRequest.FirstName;
        user.LastName = changeRequest.LastName;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> PasswordChangeAsync(PasswordChangeModel model)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.LoginNormalized == model.Login.ToLower());

        if (user == null)
        {
            return Result.Fail(MessageConstants.UserNotFound);
        }

        if (user.PasswordHash != GeneratePasswordHash(model.OldPassword))
            return Result.Fail(MessageConstants.OldPasswordNotMatch);

        user.PasswordHash = GeneratePasswordHash(model.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<SinginReponseModel>> SingInAsync(SinginRequestModel requestModel, CancellationToken ct)
    {
        var passwordHash = GeneratePasswordHash(requestModel.Password);

        var model = await _dbContext.Users
            .Where(x => x.LoginNormalized == requestModel.Login.ToLower()
                        && x.PasswordHash == passwordHash
            )
            .Select(x => new
            {
                userId = x.Id,
                userRoles = x.UserRoles.Select(u => u.Role.RoleName).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (model is null) return Result.Fail(MessageConstants.InvalidCredentials);

        var tokenModel = await _tokenService.GenerateNewTokenModelAsync(model.userId, model.userRoles, ct);

        return Result.Ok(new SinginReponseModel(
            tokenModel.AccessToken,
            tokenModel.RefreshToken
        ));
    }

    public async Task<List<UserModel>> GetUsers(CancellationToken ct)
    {
        var users = await _dbContext.Users.ToListAsync(ct);
        return users.Select(x => x.ToModel()).ToList();
    }

    public async Task<Result> MakeUserAdmin(Guid userId, CancellationToken ct)
    {
        var adminRole = await _dbContext.Roles.FirstAsync(x => x.RoleName == RoleName.Admin, ct);
        var alreadyAdmin =
            await _dbContext.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == adminRole.Id, ct);
        if (alreadyAdmin) return Result.Fail(MessageConstants.UserAlreadyAdmin);

        var userRole = new UserRoleEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = adminRole.Id
        };

        await _dbContext.UserRoles.AddAsync(userRole, ct);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }
    
    public async Task<Result> AddRoleAsync(Guid userId, RoleName role, CancellationToken ct)
    {
        var roleId = RoleConstants.RoleNameToGuid[role];
        var roleExists = await _dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (roleExists) return Result.Fail(MessageConstants.UserAlreadyHasRole);
        var userRole = new UserRoleEntity
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            UserId = userId
        };

        await _dbContext.UserRoles.AddAsync(userRole, ct);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, RoleName role, CancellationToken ct)
    {
        var roleId = RoleConstants.RoleNameToGuid[role];
        var roleExists = await _dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (!roleExists) return Result.Fail(MessageConstants.UserHasNoRole);
        // _dbContext.UserRoles.


        throw new NotImplementedException();
    }

    private static string GeneratePasswordHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    private async Task<UserEntity?> GetUserByLoginAsync(string login, CancellationToken ct)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.LoginNormalized == login.ToLower(), ct);
    }

    private Guid GetUserIdFromContext()
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
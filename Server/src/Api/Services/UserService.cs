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
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(ApplicationDbContext dbContext, ITokenService tokenService, IEmailService emailService, IHttpContextAccessor contextAccessor)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailService = emailService;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var userExists = _dbContext.Users.Any(x => x.LoginNormalized == request.Login.ToLower());
        if (userExists) return Result.Fail(MessageConstants.UserAlreadyRegistered);

        var user = new UserEntity
        {
            Age = request.Age,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            EmailNormalized = request.Email.ToLower(),
            Login = request.Login,
            LoginNormalized = request.Login.ToLower(),
            PasswordHash = GeneratePasswordHash(request.Password)
        };

        user.UserRoles.Add(new UserRoleEntity { RoleId = RoleConstants.UserRoleId });
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);
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

    public async Task<Result> ChangeAsync(ChangeRequest changeRequest, CancellationToken ct)
    {
        var userId = GetUserIdFromContext();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return Result.Fail(MessageConstants.UserNotFound);

        user.FirstName = changeRequest.FirstName;
        user.LastName = changeRequest.LastName;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> PasswordChangeAsync(PasswordChangeModel model, CancellationToken ct)
    {
        var user = await GetUserByLoginAsync(model.Login.ToLower(), ct);

        if (user == null)
        {
            return Result.Fail(MessageConstants.UserNotFound);
        }

        if (user.PasswordHash != GeneratePasswordHash(model.CurrentPassword))
            return Result.Fail(MessageConstants.OldPasswordNotMatch);

        user.PasswordHash = GeneratePasswordHash(model.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<SinginResponseModel>> SingInAsync(SinginRequestModel requestModel, CancellationToken ct)
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

        return Result.Ok(new SinginResponseModel(
            tokenModel.AccessToken,
            tokenModel.RefreshToken
        ));
    }

    public async Task<List<UserModel>> GetUsers(CancellationToken ct)
    {
        var users = await _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(u => u.Role)
            .Select(x => x.ToModel())
            .ToListAsync(ct);
        return users;
    }

    public async Task<Result> AddRoleAsync(Guid userId, RoleNames role, CancellationToken ct)
    {
        var roleId = RoleConstants.RoleNameToGuid[role];
        var roleExists = await _dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (roleExists) return Result.Fail(MessageConstants.UserAlreadyHasRole);
        var userRole = new UserRoleEntity
        {
            RoleId = roleId,
            UserId = userId
        };

        await _dbContext.UserRoles.AddAsync(userRole, ct);
        await _dbContext.SaveChangesAsync(ct);
        await _tokenService.RevokeTokens(userId, ct);
        return Result.Ok();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, RoleNames role, CancellationToken ct)
    {
        var roleId = RoleConstants.RoleNameToGuid[role];
        var existingRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, ct);
        if (existingRole is null) return Result.Fail(MessageConstants.UserHasNoRole);
        _dbContext.UserRoles.Remove(existingRole);
        await _dbContext.SaveChangesAsync(ct);

        await _tokenService.RevokeTokens(userId, ct);
        return Result.Ok();
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

    public async Task<Result> SendRecoveryLinkAsync(string email, CancellationToken ct)
    {
        var userId = await _dbContext.Users
            .Where(x => x.EmailNormalized == email.ToLower())
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);
        if (userId == Guid.Empty) return Result.Fail(MessageConstants.EmailNotFound);
        await _emailService.SendRecoveryLink(userId, email, ct);
        return Result.Ok();
    }
}


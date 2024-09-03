using API.Models.Request;
using API.Models.Response;
using API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using API.Models;
using API.Models.Entitites;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public UserService(ApplicationDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var userExists = _dbContext.Users.Any(x => x.LoginNormalized == request.Login.ToLower());
        if (userExists) return new RegisterResponse(null);

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
        return new RegisterResponse(user.Id);
    }

    public async Task<Result> DeleteAsync(string login, CancellationToken ct)
    {
        var user = await GetUserByLoginAsync(login, ct);
        if (user is null) return Result.Fail("User doesn't exist");

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public Task<Result> ChangeAsync(ChangeRequest toRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> PasswordChangeAsync(PasswordChangeModel model)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.LoginNormalized == model.Login.ToLower());

        if (user == null)
        {
            return Result.Fail("User not found");
        }

        if (user.PasswordHash != GeneratePasswordHash(model.OldPassword)) return Result.Fail("Old password doesn't match");

        user.PasswordHash = GeneratePasswordHash(model.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<SinginReponseModel>> SinginAsync(SinginRequestModel requestModel)
    {
        var user = await GetUserByLoginAsync(requestModel.Login, CancellationToken.None);
        if (user is null) return Result.Fail("User not found");
        
        var passwordCheck = user.PasswordHash == GeneratePasswordHash(requestModel.Password);

        return passwordCheck ? Result.Ok(new SinginReponseModel(_tokenService.GenerateAuthToken(user))) 
            : Result.Fail("Invalid credentials");
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
                x => string.Equals(x.Value, ClaimTypes.NameIdentifier,
                    StringComparison.InvariantCultureIgnoreCase))?.Value;
        if (!Guid.TryParse(nameIdentifier, out var userId))
        {
            throw new ArgumentNullException(nameof(ClaimTypes.NameIdentifier));
        }
        return userId;
    }
}
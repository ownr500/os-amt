using API.Models.Request;
using API.Models.Response;
using API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using API.Models;
using API.Models.Entitites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
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

    public async Task<DeleteResponse> DeleteAsync(string login)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Login == login);
        if (user is null) return new DeleteResponse(null);

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return new DeleteResponse(user.Id);
    }

    public Task<ChangeResponse> ChangeAsync(ChangeRequest toRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> PasswordChangeAsync(PasswordChangeModel model)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.LoginNormalized == model.Login.ToLower());

        if (user == null)
        {
            return false;
        }
        if (user.PasswordHash != GeneratePasswordHash(model.OldPassword)) return false;
        
        user.PasswordHash = GeneratePasswordHash(model.NewPassword);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return true;
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
}
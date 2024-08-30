using API.Models.Request;
using API.Models.Response;
using API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using API.Models;
using API.Models.Entitites;

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
        var user = new User
        {
            Id = Guid.NewGuid(),
            Age = request.Age,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Login = request.Login,
            PasswordHash = GeneratePasswordHash(request.Password)
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return new RegisterResponse(user.Id);
    }

    private static string GeneratePasswordHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        StringBuilder builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
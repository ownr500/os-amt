using System.Security.Claims;
using API.Models.Entitites;

namespace API.Services.Interfaces;

public interface ITokenService
{
    string GenerateAuthToken(UserEntity user);
    ClaimsPrincipal ValidateToken(string token);
    Task SaveToken(string token);
}
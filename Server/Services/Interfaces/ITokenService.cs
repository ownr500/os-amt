using System.Security.Claims;
using API.Models.Entities;

namespace API.Services.Interfaces;

public interface ITokenService
{
    string GenerateAuthToken(UserEntity user);
    Task<ClaimsPrincipal> ValidateToken(string token);
    Task AddTokenAsync(string token);
}
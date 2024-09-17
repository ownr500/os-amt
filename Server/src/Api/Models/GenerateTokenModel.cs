using System.Security.Claims;

namespace API.Models;

public record GenerateTokenModel(
    Guid UserId, 
    List<Claim> Claims, 
    DateTime ExpireAt,
    string SecretKey,
    string Audience,
    string Issuer
);
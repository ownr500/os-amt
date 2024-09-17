using System.Security.Claims;
using API.Options;

namespace API.Models;

public record GenerateTokenModel(
    Guid UserId,
    List<Claim> Claims,
    TokenInfo TokenInfo
);

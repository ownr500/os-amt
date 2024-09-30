using System.Security.Claims;

namespace API.Core.Models;

public record RecoveryTokenModel(
    Guid UserId,
    ClaimsPrincipal Claims,
    DateTime ExpireAt);
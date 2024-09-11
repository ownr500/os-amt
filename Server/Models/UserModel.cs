using API.Models.enums;

namespace API.Models;

public record UserModel(
    Guid UserId,
    string FirstName,
    string LastName,
    string Login,
    List<RoleNames> Roles
    );
using API.Models.enums;

namespace API.Controllers.DTO;

public record UserDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Login,
    List<RoleType> Role
);
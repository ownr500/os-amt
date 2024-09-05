using API.Models.enums;

namespace API.Controllers.DTO;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Login,
    List<RoleName> Roles
);
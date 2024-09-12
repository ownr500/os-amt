namespace API.Controllers.DTO;

public record UserToAdminDto(
    string Login,
    string SuperPassword);
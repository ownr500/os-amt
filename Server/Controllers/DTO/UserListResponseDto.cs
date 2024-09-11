namespace API.Controllers.DTO;

public record UserListResponseDto(
    List<UserDto> UsersDtos);
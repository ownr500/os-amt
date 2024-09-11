namespace API.Controllers.DTO;

public record SinginResponseDto(
    string AccessToken,
    string RefreshToken
);
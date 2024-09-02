namespace API.Controllers.DTO;

public record SinginResponseDto(
    string JwtToken,
    string? ErrorMessage
);
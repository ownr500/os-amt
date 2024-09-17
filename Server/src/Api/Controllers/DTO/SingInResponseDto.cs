namespace API.Controllers.DTO;

public sealed record SingInResponseDto(
    string AccessToken,
    string RefreshToken
);
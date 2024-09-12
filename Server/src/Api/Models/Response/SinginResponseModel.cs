namespace API.Models.Response;

public record SinginResponseModel(
    string AccessToken,
    string RefreshToken);
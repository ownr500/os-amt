namespace API.Models.Response;

public record TokenModel(
    string AccessToken,
    string RefreshToken);
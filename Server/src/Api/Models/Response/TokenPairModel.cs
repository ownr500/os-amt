namespace API.Models.Response;

public record TokenPairModel(
    string AccessToken,
    string RefreshToken);
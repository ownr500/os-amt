namespace API.Models.Response;

public record SinginReponseModel(
    string AccessToken,
    string RefreshToken);
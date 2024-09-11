namespace API.Models.Request;

public record PasswordChangeModel(
    string Login,
    string CurrentPassword,
    string NewPassword
    );
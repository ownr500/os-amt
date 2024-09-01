namespace API.Models.Request;

public record PasswordChangeModel(
    string Login,
    string OldPassword,
    string NewPassword
    );
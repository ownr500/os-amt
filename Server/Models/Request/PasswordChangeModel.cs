namespace API.Models.Request;

public record PasswordChangeModel(
    Guid? Id,
    string Login,
    string OldPassword,
    string NewPassword
    );
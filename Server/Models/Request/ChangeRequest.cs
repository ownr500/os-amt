namespace API.Models.Request;

public record ChangeRequest(
    string FirstName,
    int Age,
    string Password
);
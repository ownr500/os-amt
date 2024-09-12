namespace API.Models.Request;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    int Age,
    string Login,
    string Password
);
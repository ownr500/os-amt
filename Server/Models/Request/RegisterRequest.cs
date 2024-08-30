namespace API.Models.Request;

public record RegisterRequest(
    string FirstName,
    string LastName,
    int Age,
    string Login,
    string Password
);
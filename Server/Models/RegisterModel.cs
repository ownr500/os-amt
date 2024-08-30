namespace API.Models;

public record RegisterModel(
    string FirstName,
    string LastName,
    int Age,
    string Login,
    string Password
);
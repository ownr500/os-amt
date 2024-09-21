namespace API.Core.Models;

public record UpdateFirstLastNameRequest(
    string FirstName,
    string LastName
);
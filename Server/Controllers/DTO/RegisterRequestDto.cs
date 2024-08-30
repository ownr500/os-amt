using System.Text.Json.Serialization;

namespace API.Controllers.DTO;

public record RegisterRequestDto(
    [property: JsonPropertyName("firstName")]
    string FirstName,
    [property: JsonPropertyName("lastName")]
    string LastName,
    [property: JsonPropertyName("age")]
    int Age,
    [property: JsonPropertyName("login")]
    string Login,
    [property: JsonPropertyName("password")]
    string Password
);
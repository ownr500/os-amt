using System.Text.Json.Serialization;

namespace API.Controllers.DTO;

public record RegisterResponseDto(
    [property: JsonPropertyName("success")]
    bool Success,
    [property: JsonPropertyName("errorMessage")]
    string? ErrorMessage
);
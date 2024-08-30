using System.Text.Json.Serialization;

namespace API.Controllers.DTO;

public record DeleteResponseDto(
    [property: JsonPropertyName("success")]
    bool Success,
    [property: JsonPropertyName("errorMessage")]
    string? ErrorMessage
);
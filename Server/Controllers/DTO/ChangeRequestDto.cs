using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.DTO;

public record ChangeRequestDto(
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    [property: JsonPropertyName("firstName")]
    string FirstName,
    [property: JsonPropertyName("age")]
    [Range(ValidationConstants.MinAge, int.MaxValue)]
    int Age,
    [property: JsonPropertyName("password")]
    [StringLength(ValidationConstants.MaxPasswordLength, MinimumLength = ValidationConstants.MinPasswordLength)]
    string Password
);
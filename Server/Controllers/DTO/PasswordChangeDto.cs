﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.DTO;

public record PasswordChangeDto(
    [property: JsonPropertyName("login")]
    [StringLength(ValidationConstants.MaxLoginLength, MinimumLength = ValidationConstants.MinLoginLength)]
    string Login,
    [property: JsonPropertyName("oldPassword")]
    [StringLength(ValidationConstants.MaxPasswordLength, MinimumLength = ValidationConstants.MinPasswordLength)]
    string OldPassword,
    [property: JsonPropertyName("newPassword")]
    [StringLength(ValidationConstants.MaxPasswordLength, MinimumLength = ValidationConstants.MinPasswordLength)]
    string NewPassword
);
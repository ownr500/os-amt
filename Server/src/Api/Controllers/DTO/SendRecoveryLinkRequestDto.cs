using System.ComponentModel.DataAnnotations;
using API.Constants;

namespace API.Controllers.DTO;

public record SendRecoveryLinkRequestDto(
    [EmailAddress]
    [StringLength(ValidationConstants.MaxEmailLength, MinimumLength = ValidationConstants.MinEmailLength)]
    string Email);
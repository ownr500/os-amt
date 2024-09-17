using System.ComponentModel.DataAnnotations;
using API.Constants;

namespace API.Controllers.DTO;

public record SendRecoveryEmailRequestDto(
    [EmailAddress]
    [MaxLength(ValidationConstants.MaxEmailLength)]
    string Email);
using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entities;

[Index(nameof(AccessToken), IsUnique = true)]
[Index(nameof(RefreshToken), IsUnique = true)]
public class RevokedTokenEntity
{
    [Key] public Guid Id { get; set; }
    
    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string AccessToken { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string RefreshToken { get; set; } = string.Empty;
    
    public DateTimeOffset RefreshTokenExpireAt { get; set; }
}
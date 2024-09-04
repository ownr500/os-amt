using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entities;

[Index(nameof(AuthToken), IsUnique = true)]
[Index(nameof(RefreshToken), IsUnique = true)]
public class TokenEntity
{
    [Key] public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string AuthToken { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string RefreshToken { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTimeOffset RefreshTokenExpireAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public virtual UserEntity User { get; set; } = default!;
}
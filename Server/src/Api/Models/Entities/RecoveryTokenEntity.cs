using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entities;

[Index(nameof(Token), IsUnique = true)]
public class RecoveryTokenEntity
{
    [Key] public Guid Id { get; set; }
    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string Token { get; set; } = string.Empty;
    
    public DateTimeOffset ExpireAt { get; set; }
    public bool IsActive { get; set; }
}
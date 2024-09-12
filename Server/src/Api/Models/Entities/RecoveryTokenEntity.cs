using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entities;

[Index(nameof(RecoveryToken), IsUnique = true)]
public class RecoveryTokenEntity
{
    [Key] public Guid Id { get; set; }
    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string RecoveryToken { get; set; } = string.Empty;
    
    public DateTimeOffset RecoveryTokenExpireAt { get; set; }
}
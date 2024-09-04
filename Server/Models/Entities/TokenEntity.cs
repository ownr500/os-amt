using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entities;

[Index(nameof(AuthToken), IsUnique = true)]
public class TokenEntity
{
    [Key] public Guid Id { get; set; }

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string AuthToken { get; set; }
}
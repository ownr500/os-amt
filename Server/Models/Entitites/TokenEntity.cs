using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entitites;

[Index(nameof(JwtToken), IsUnique = true)]
public class TokenEntity
{
    [Key] public Guid Id { get; set; }

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string JwtToken { get; set; }
}
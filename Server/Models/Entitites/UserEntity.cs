using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Entitites;

[Index(nameof(LoginNormalized), IsUnique = true)]
public class UserEntity
{
    [Key] public Guid Id { get; set; }

    [MaxLength(ValidationConstants.MaxLengthName)]
    public string FirstName { get; set; }

    [MaxLength(ValidationConstants.MaxLengthName)]
    public string LastName { get; set; }

    public int Age { get; set; }

    [MaxLength(ValidationConstants.MaxLoginLength)]
    public string Login { get; set; }

    [MaxLength(ValidationConstants.MaxLoginLength)]
    public string LoginNormalized { get; set; }

    [MaxLength(ValidationConstants.MaxPasswordHashLength)]
    public string PasswordHash { get; set; }
}
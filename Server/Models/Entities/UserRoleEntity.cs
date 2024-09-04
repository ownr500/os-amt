using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities;

public class UserRoleEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public UserEntity User { get; set; }
    
    public Guid RoleId { get; set; }
    public RoleEntity Role { get; set; }
}
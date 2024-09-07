using System.ComponentModel.DataAnnotations;

namespace API.Models.Entities;

public class UserRoleEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public virtual UserEntity User { get; set; }
    
    public Guid RoleId { get; set; }
    public virtual RoleEntity Role { get; set; }
}
using System.ComponentModel.DataAnnotations;
using API.Models.enums;

namespace API.Models.Entities;

public class RoleEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public RoleName RoleName { get; set; }
    
    public ICollection<UserRoleEntity> UserRoles { get; set; }
}
using System.ComponentModel.DataAnnotations;
using API.Models.enums;

namespace API.Models.Entities;

public class RoleEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public RoleNames RoleNames { get; set; }

    public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
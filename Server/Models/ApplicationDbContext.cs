using API.Constants;
using API.Models.Entities;
using API.Models.enums;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; } = default!;
    public DbSet<TokenEntity> Tokens { get; set; } = default!;
    public DbSet<RoleEntity> Roles { get; set; } = default!;
    public DbSet<UserRoleEntity> UserRoles { get; set; } = default!;
    public DbSet<RevokedTokenEntity> RevokedTokens { get; set; } = default!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TokenEntity>()
            .HasOne(x => x.User)
            .WithMany(x => x.Tokens)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<RoleEntity>().HasData(
            new RoleEntity
            {
                Id = RoleConstants.AdminRoleId,
                RoleNames = RoleNames.Admin
            },
            new RoleEntity
            {
                Id = RoleConstants.UserRoleId,
                RoleNames = RoleNames.User
            });

        modelBuilder.Entity<UserEntity>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<RoleEntity>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId);
        
        base.OnModelCreating(modelBuilder);
    }
}
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
    public DbSet<RecoveryTokenEntity> RecoveryTokens { get; set; } = default!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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
                RoleName = RoleNames.Admin
            },
            new RoleEntity
            {
                Id = RoleConstants.UserRoleId,
                RoleName = RoleNames.User
            });

        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity
            {
                Id = Guid.Parse("561BBFAA-C44A-45F9-97C4-7182BA38B85F"),
                Age = 30,
                Login = "admin",
                FirstName = "Admin",
                LastName = "Admin",
                LoginNormalized = "admin",
                PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5"
            }
        );

        modelBuilder.Entity<UserRoleEntity>().HasData(
            new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("561BBFAA-C44A-45F9-97C4-7182BA38B85F"),
                RoleId = RoleConstants.AdminRoleId
            }
        );

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
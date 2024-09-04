using API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; } = default!;
    public DbSet<TokenEntity> Tokens { get; set; } = default!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TokenEntity>()
            .HasOne(x => x.User)
            .WithMany(x => x.Tokens)
            .HasForeignKey(x => x.UserId);
        
        base.OnModelCreating(modelBuilder);
    }
}
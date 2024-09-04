using API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TokenEntity> Tokens { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {}
}
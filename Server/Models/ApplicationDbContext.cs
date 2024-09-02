﻿using API.Models.Entitites;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {}
}
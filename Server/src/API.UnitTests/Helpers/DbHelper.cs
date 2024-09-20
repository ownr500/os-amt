using API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.UnitTests.Helpers;

public class DbHelper
{

    public static ApplicationDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseInMemoryDatabase(databaseName: "ApplicationDbContextInMemory");

        var dbContextOptions = builder.Options;
        var dbContext = new ApplicationDbContext(dbContextOptions);
        
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        dbContext.SaveChanges();
        
        return dbContext;
    }
}
using Company.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Company.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void RegisterDbContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));
    }
}
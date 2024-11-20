using Company.Configurations;
using Company.Core.Options;
using Company.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Company.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void RegisterDbContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));
    }

    public static void RegisterConfigurationOptions(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
        builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
    }

    public static void RegisterOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection(nameof(TokenOptions)));
    }
    
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        
    }

    public static void RegisterMassTransit(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");
            });
        });
    }
}
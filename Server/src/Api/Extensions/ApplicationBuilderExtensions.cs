using System.IdentityModel.Tokens.Jwt;
using API.Configurations;
using API.Core.Options;
using API.Core.Services;
using API.Implementation.Services;
using API.Infrastructure;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<JwtSecurityTokenHandler>();
        builder.Services.AddScoped<RevokedTokenMiddleware>();
    }

    public static void RegisterOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(nameof(SmtpOptions)));
        builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection(nameof(TokenOptions)));
    }

    public static void RegisterDbContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));
    }

    public static void RegisterConfigurationOptions(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
        builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
        builder.Services.ConfigureOptions<ConfigureAuthenticationOptions>();
    }
}
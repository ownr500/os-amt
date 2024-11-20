using System.Text.Json.Serialization;
using Company.Extensions;
using Company.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterConfigurationOptions();
builder.RegisterServices();
builder.RegisterMassTransit();
builder.RegisterOptions();
builder.RegisterDbContext();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAuthentication().AddJwtBearer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RevokedTokenMiddleware>();
app.MapControllers();

await app.RunAsync();
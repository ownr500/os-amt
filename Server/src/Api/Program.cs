using System.Text.Json.Serialization;
using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();
builder.RegisterOptions();
builder.RegisterDbContext();
builder.RegisterServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddAuthentication()
    .AddJwtBearer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RevokedTokenMiddleware>();
app.MapControllers();

await app.RunAsync();
using System.Text.Json.Serialization;
using Company.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterConfigurationOptions();
builder.RegisterServices();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
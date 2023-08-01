using Chinook.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConnectionProvider(builder.Configuration);
builder.Services.ConfigureRepositories();
builder.Services.ConfigureSupervisor();
builder.Services.AddAPILogging();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

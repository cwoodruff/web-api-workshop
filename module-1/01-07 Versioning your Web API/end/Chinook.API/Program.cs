using Chinook.API.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConnectionProvider(builder.Configuration);
builder.Services.ConfigureRepositories();
builder.Services.ConfigureSupervisor();
builder.Services.AddAPILogging();
builder.Services.AddCORS();
builder.Services.ConfigureValidators();
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddControllers();

// API Versioning configuration
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Versioned API Explorer (for Swagger grouping later)
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // e.g., v1, v1.1
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();

app.UseResponseCaching();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

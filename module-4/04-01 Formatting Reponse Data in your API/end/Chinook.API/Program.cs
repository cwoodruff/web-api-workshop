using Chinook.API.Configurations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConnectionProvider(builder.Configuration);
builder.Services.ConfigureRepositories();
builder.Services.ConfigureSupervisor();
builder.Services.AddAPILogging();
builder.Services.AddCORS();
builder.Services.ConfigureValidators();
builder.Services.AddCaching(builder.Configuration);
//builder.Services.AddIdentity(builder.Configuration);
builder.Services.AddVersioning();
builder.Services.AddApiExplorer();
builder.Services.AddSwaggerServices();
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlSerializerFormatters()
.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();

app.UseCors();

// Prevent caching for Swagger assets and JSON to avoid HTML being cached under the same URL
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers["Cache-Control"] = "no-store, no-cache";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
    await next();
});

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger(c =>
    {
        // Make the Swagger JSON route explicit and predictable
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            // Use an absolute path to avoid any relative resolution issues in the UI
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Chinook API {description.GroupName.ToUpperInvariant()}");
        }
        // Serve Swagger UI from /docs to keep /swagger solely for JSON endpoints and avoid any route overlap
        options.RoutePrefix = "docs";
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();

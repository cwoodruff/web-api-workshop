using System.Runtime.InteropServices;
using System.Text;
using Chinook.API.Configurations;
using Chinook.API.Data;
using Chinook.API.Identity;
using Chinook.API.Infrastructure;
using Chinook.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

// Identity EF DbContext (separate Identity database with provider switch)
var idProvider = builder.Configuration["Identity:Provider"] ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (idProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConn = builder.Configuration.GetConnectionString("IdentitySqlite")
                         ?? "Data Source=ChinookIdentity.db;Cache=Shared";
        options.UseSqlite(sqliteConn);
    }
    else
    {
        // Default to SQL Server for Identity DB; fallback to existing Chinook connections if specific one absent
        var sqlConn = builder.Configuration.GetConnectionString("IdentitySqlServer")
                      ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                            ? builder.Configuration.GetConnectionString("ChinookDbWindows")
                            : builder.Configuration.GetConnectionString("ChinookDbDocker"));
        options.UseSqlServer(sqlConn);
    }
});

// Identity Core + Roles
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var cfg = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = cfg["Jwt:Issuer"],
            ValidAudience = cfg["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:SigningKey"] ?? "")),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanWrite", policy => policy.RequireRole("Admin", "Manager"));
});

// Token service for issuing JWTs
builder.Services.AddSingleton<ITokenService, TokenService>();

// Swagger with JWT support

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();

app.UseResponseCaching();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

// Seed Identity roles and default admin user for workshop
await IdentityDataSeeder.SeedAsync(app.Services);

app.Run();

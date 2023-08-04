---
order: 5
icon: gear
---
# Developing the API Data access

## START FROM PREVIOUS MODULE'S END
[Designing the NTier API](ntier-api-endpoints.md)

## ADD PROJECT REFERENCE FOR API PROJECT

![](ntier-data-access/2023-07-27_10-28-16.png)

## ADD PROJECT REFERENCE FOR DOMAIN PROJECT

![](ntier-data-access/2023-07-27_10-29-13.png)

## INSTALL OR UPDATE THR EF CORE CLI

```dos
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```


## ADD NUGET PACKAGES TO DATA PROJECT

```dos
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```


## SCAFFOLD DBCONTEXT AND ENTITIES FROM CHINOOK DATABASE IN DATA PROJECT

Run the following command in Developer Command Prompt inside the Data project

```dos
dotnet ef dbcontext scaffold "Server=.;Database=Chinook;MultipleActiveResultSets=true;TrustServerCertificate=true;Integrated Security=true;Trusted_Connection=true;" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Entities
```

![](ntier-data-access/2023-07-31_04-10-36.png)

## MOVE ENTITIES IN DATA PROJECT TO DOMAIN

![](ntier-data-access/2023-07-31_04-10-36.png)


Adjust the namespace for the Entities

![](ntier-data-access/2023-07-31_04-11-28.png)

Add using reference in DBContext to Entities in Domain

![](ntier-data-access/2023-07-31_04-13-37.png)

## ADD REPOSITORY INTERFACES TO DOMAIN

![](ntier-data-access/2023-07-31_04-21-40.png)

```csharp
using Chinook.Domain.Entities;

namespace Chinook.Domain.Repositories
{
    public interface IAlbumRepository : IDisposable
    {
        Task<List<Album>> GetAll();
        Task<Album> GetById(int id);
        Task<List<Album>> GetByArtistId(int id);
        Task<Album> Add(Album newAlbum);
        Task<bool> Update(Album album);
        Task<bool> Delete(int id);
    }
}
```

## ADD REPOSITORIES TO DATA

![](ntier-data-access/2023-07-31_04-23-11.png)

```csharp
using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly ChinookContext _context;

        public AlbumRepository(ChinookContext context)
        {
            _context = context;
        }

        private async Task<bool> AlbumExists(int id) =>
            await _context.Albums.AnyAsync(a => a.Id == id);

        public void Dispose() => _context.Dispose();

        public async Task<List<Album>> GetAll() => await _context.Albums.AsNoTrackingWithIdentityResolution().ToListAsync();

        public async Task<Album> GetById(int id)
        {
            var dbAlbum = await _context.Albums.FindAsync(id);
            return dbAlbum;
        }

        public async Task<Album> Add(Album newAlbum)
        {
            await _context.Albums.AddAsync(newAlbum);
            await _context.SaveChangesAsync();
            return newAlbum;
        }

        public async Task<bool> Update(Album album)
        {
            if (!await AlbumExists(album.Id))
                return false;
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            if (!await AlbumExists(id))
                return false;
            var toRemove = await _context.Albums.FindAsync(id);
            _context.Albums.Remove(toRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Album>> GetByArtistId(int id) =>
            await _context.Albums.Where(a => a.ArtistId == id).AsNoTrackingWithIdentityResolution().ToListAsync();
    }
}
```

## ADD CONFIGURATIONS FOLDER to API PROJECT

![](ntier-data-access/2023-07-31_04-24-29.png)

## ADD CONNECTIONSTRING TO APPSETTINGS.JSON

```json
"ConnectionStrings": {
  "ChinookDbWindows": "Server=.;Database=Chinook;MultipleActiveResultSets=true;TrustServerCertificate=true;Integrated Security=true;Trusted_Connection=true;Application Name=Chinook7WebAPI",
  "ChinookDbDocker": "Server=localhost,1433;Database=Chinook;User=sa;Password=P@55w0rd;MultipleActiveResultSets=true;TrustServerCertificate=true;Integrated Security=true;Trusted_Connection=true;Application Name=Chinook7WebAPI"
},
```

## ADD APPSETTINGS TO API PROJECT IN CONFIGURATIONS FOLDER

```csharp
namespace Chinook.API.Configurations;

public abstract class AppSettings
{
}
```

## ADD CONFIGUREAPPSETTINGS TO API PROJECT
![](ntier-data-access/2023-07-31_04-28-46.png)


### CONFIGUREAPPSETTINGS CLASS IN CONFIGURATIONS FOLDER

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chinook.API.Configurations
{
    public static class ConfigureAppSettings
    {
        public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(_ => configuration.GetSection("AppSettings").Bind(_));

            return services;
        }
    }
}
```

### ADD AddAppSettings TO PROGRAM.CS IN API PROJECT
```csharp
using Chinook.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddControllers();
```

## ADD DBCONTEXT TO DEPENDANCY INJECTION IN API PROJECT

![](ntier-data-access/2023-07-31_04-30-14.png)

```csharp
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Chinook.Data.Data;

namespace Chinook.API.Configurations;

public static class ConfigureConnections
{
    public static IServiceCollection AddConnectionProvider(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connection = String.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            connection = configuration.GetConnectionString("ChinookDbWindows");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            connection = configuration.GetConnectionString("ChinookDbDocker");

        services.AddDbContextPool<ChinookContext>(options => options.UseSqlServer(connection));
        services.AddSingleton(new SqlConnection(connection));

        return services;
    }
}
```

## CALL ADDCONNECTIONPROVIDER() IN CONFIGURESERVICES IN API PROJECT'S StartUP

```csharp
builder.Services.AddConnectionProvider(builder.Configuration);
```

## REMOVE ONCONFIGURING() FROM DBCONTEXT
![](ntier-data-access/2023-07-31_04-39-34.png)

## REMOVE THE DBCONTEXT CONSTRUCTOR W/O A PARAMETER
![](ntier-data-access/2023-07-31_04-40-02.png)

## REMOVE CLASS.CS FILES FROM DATA AND DOMAIN PROJECTS

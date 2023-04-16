---
order: 1
icon: cpu
---
# Consuming ASP.NET Web API with OpenAPI Document

## INSTALL .NET OPENAPI TOOL

Microsoft.dotnet-openapi is a .NET Core Global Tool for managing OpenAPI references within a project.

```csharp
dotnet tool install -g Microsoft.dotnet-openapi
```

## CREATE NEW CONSOLE PROJECT

```dos
dotnet new console --name OpenAPIClient
```

## GET SWAGGER DOC FROM YOUR WEB APIs

### Run your Web API

![](consuming-web-api-with-openapi/2023-01-08_09-54-19.png)

## ADD OPENAPI CONNECTED SERVICE TO PROJECT

![](consuming-web-api-with-openapi/2023-01-08_09-50-48.png)


![](consuming-web-api-with-openapi/2023-01-08_09-51-52.png)

![](consuming-web-api-with-openapi/2023-01-08_09-52-10.png)

![](consuming-web-api-with-openapi/2023-01-08_12-12-57.png)

![](consuming-web-api-with-openapi/2023-01-08_09-59-52.png)



## WRITE CODE TO USE OPENAPI

```csharp
using ChinookOpenAPI;

using var httpClient = new HttpClient();

var apiClient = new ChinookOpenAPIClient("https://localhost:7011/", httpClient);

var genres = await apiClient.GenreAllAsync(1, 20, "");

if (genres != null)
    foreach (var genre in genres)
    {
        Console.WriteLine(genre.Name);
    }
Console.ReadLine();
```

![](consuming-web-api-with-openapi/2023-01-08_10-35-05.png)


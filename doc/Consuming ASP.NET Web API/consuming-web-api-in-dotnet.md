---
order: 3
icon: cpu
---
# Consuming ASP.NET Web API in .NET

## CREATE NEW CONSOLE PROJECT

```dos
dotnet new console --name ChinookAPIClient
```

ADD the following async method to the Program class 

```csharp
static Task ProcessRepositories()
{
    
}
```
## CREATE NEW STATIC INSTANCE OF HttpClinet

```csharp
// See https://aka.ms/new-console-template for more information

HttpClient client = new HttpClient();
```

## REPLACE MAIN METHOD

```csharp
// See https://aka.ms/new-console-template for more information

HttpClient client = new HttpClient();

await ProcessRepositories(client);
```

## ADD API CALL TO ProcessRepositories

```csharp
static async Task ProcessRepositories(HttpClient client)
{
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Add("User-Agent", ".NET Console");

    var stringTask = client.GetStringAsync("https://localhost:7211/api/v1/Customer");

    var msg = await stringTask;
    Console.Write(msg);
}
```

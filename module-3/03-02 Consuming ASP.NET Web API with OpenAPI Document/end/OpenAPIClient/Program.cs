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

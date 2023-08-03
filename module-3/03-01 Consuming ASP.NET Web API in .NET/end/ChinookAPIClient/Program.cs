// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;

HttpClient client = new HttpClient();

await ProcessRepositories(client);

static async Task ProcessRepositories(HttpClient client)
{
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Add("User-Agent", ".NET Console");

    var stringTask = client.GetStringAsync("https://localhost:7211/api/v1/Customer");

    var msg = await stringTask;
    Console.Write(msg);
}
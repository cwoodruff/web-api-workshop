using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chinook.ImplementationTests.API;

public class AlbumApiTest : IDisposable
{
    private readonly HttpClient _client;

    public AlbumApiTest()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // ... Configure test services
            });

        _client = application.CreateClient();
    }
    
    public void Dispose()
    {
        _client.Dispose();
    }

    [Theory]
    [InlineData("GET")]
    public async void AlbumGetAllTest(string method)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/Album/");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", 1)]
    public async Task AlbumGetTest(string method, int? id = null)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/Album/{id}");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
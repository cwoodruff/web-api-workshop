using System;
using System.Linq;
using System.Threading.Tasks;
using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Repositories;
using Chinook.Domain.Supervisor;
using Chinook.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Chinook.UnitTests.Supervisor;

public class AlbumSupervisorTest : IDisposable
{
    private readonly IChinookSupervisor _super;
    private readonly TestHelper _helper;
    private readonly IAlbumRepository _albumRepo;
    private readonly IArtistRepository _artistRepo;
    private readonly ITrackRepository _trackRepo;
    private readonly ChinookContext _context;

    public AlbumSupervisorTest()
    {
        _helper = new TestHelper();
        _context = _helper.Context;
        _albumRepo = _helper.AlbumInMemoryRepository();
        _artistRepo = _helper.ArtistInMemoryRepository();
        _trackRepo = _helper.TrackInMemoryRepository();
            
        _super = new ChinookSupervisor(_albumRepo, _artistRepo, null, null,
            null, null, null, null,
            null, null, null, null,
            null, null, null, null, null,
            null, null, null, new MemoryCache(new MemoryCacheOptions()), null);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _albumRepo?.Dispose();
        _artistRepo?.Dispose();
    }

    [Fact]
    public async Task GetAllAlbum_GivenTwoAlbumsInTheDatabase_ReturnsBoth()
    {
        var artist = new Artist() { Id = 1, Name = "Artist1" };
        var track1 = new Track() { Id = 1, Name = "Track1", AlbumId = 12};
        var track2 = new Track() { Id = 2, Name = "Track2", AlbumId = 123};
        var album1 = new Album { Id = 12, Title = "Title1", ArtistId = 1};
        var album2 = new Album { Id = 123, Title = "Title1", ArtistId = 1};

        // Arrange
        _context.Artists.Add(artist);
        _context.Albums.Add(album1);
        _context.Albums.Add(album2);
        //_context.Tracks.Add(track1);
        //_context.Tracks.Add(track2);
        await _context.SaveChangesAsync();

        // Act
        var albums = (await _super.GetAllAlbum()).ToList();

        // Assert
        //albums.Count.Should().Be(2);
        albums.Should().Contain(x => x.Id == 12);
        albums.Should().Contain(x => x.Id == 123);
    }

    [Fact]
    public async Task GetAlbumByID_MatchingAlbumInDB_ReturnsIt()
    {
        // Arrange
        var artist = new Artist() { Id = 6, Name = "Artist1" };
        var track1 = new Track() { Id = 7, Name = "Track1", AlbumId = 55};
        var track2 = new Track() { Id = 8, Name = "Track2", AlbumId = 56};
        var album1 = new Album { Id = 55, Title = "Title1", ArtistId = 6};
        var album2 = new Album { Id = 56, Title = "Title1", ArtistId = 6};

        // Arrange
        _context.Artists.Add(artist);
        _context.Albums.Add(album1);
        _context.Albums.Add(album2);
        _context.Tracks.Add(track1);
        _context.Tracks.Add(track2);
        await _context.SaveChangesAsync();

        // Act
        var albums = (await _super.GetAllAlbum()).ToList();
        var album = await _super.GetAlbumById(55);

        // Assert
        album.Id.Should().Be(55);
    }
}
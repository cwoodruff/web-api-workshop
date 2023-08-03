using System;
using System.Threading.Tasks;
using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Repositories;
using Chinook.UnitTests.Helpers;
using FluentAssertions;
using Xunit;

namespace Chinook.UnitTests.Repositories
{
    public class AlbumRepositoryTest : IDisposable
    {
        private readonly TestHelper _helper;
        private readonly IAlbumRepository _repo;
        private readonly ChinookContext _context;

        public AlbumRepositoryTest()
        {
            _helper = new TestHelper();
            _context = _helper.Context;
            _repo = _helper.AlbumInMemoryRepository();
        }
        
        public void Dispose()
        {
            _repo?.Dispose();
        }

        [Fact]
        public async Task AlbumGetAll()
        {
            var artist = new Artist() { Id = 1, Name = "Artist1" };
            var album1 = new Album { Id = 12, Title = "Title1", ArtistId = 1};
            var album2 = new Album { Id = 123, Title = "Title1", ArtistId = 1};

            // Arrange
            _context.Artists.Add(artist);
            _context.Albums.Add(album1);
            _context.Albums.Add(album2);
            _context.SaveChanges();

            // Act
            var albums = await _repo.GetAll();

            // Assert
            albums.Count.Should().Be(2);
            albums.Should().Contain(x => x.Id == 12);
            albums.Should().Contain(x => x.Id == 123);
        }

        [Fact]
        public async Task AlbumGetOne()
        {
            // Arrange
            var albumId = 1;
            var artistId = 1;

            // We are currently required to care about an Artist ID because the convert part of album specifically references the artist repository as well.
            _context.Artists.Add(new Artist() { Id = artistId, Name = "Artist"});
            _context.Albums.Add(new Album() { Id = albumId, Title = "Title", ArtistId = artistId });
            _context.SaveChanges();

            // Act
            var album = await _repo.GetById(albumId);

            // Assert
            album.Id.Should().Be(albumId);
        }
        
        [Fact]
        public async Task AlbumAddOne()
        {
            // Arrange
            var albumId = 42;
            var album42 = new Album { Id = albumId, Title = "Title1", ArtistId = 1};

            // Act
            _repo.Add(album42);
            var album = await _repo.GetById(albumId);

            // Assert
            album.Id.Should().Be(albumId);
        }
        
        [Fact]
        public async Task AlbumUpdateOne()
        {
            // Arrange
            var albumId = 43;
            var albumTitle = "NewTitle";
            var album43 = new Album { Id = albumId, Title = "Title1", ArtistId = 1};
            _repo.Add(album43);

            // Act
            album43.Title = albumTitle;
            _repo.Update(album43);
            var album = await _repo.GetById(albumId);

            // Assert
            album.Title.Should().Be(albumTitle);
        }
        
        [Fact]
        public async Task AlbumDeleteOne()
        {
            // Arrange
            var albumId = 44;
            var album44 = new Album { Id = albumId, Title = "Title1", ArtistId = 1};
            _repo.Add(album44);

            // Act
            _repo.Delete(albumId);
            var album = await _repo.GetById(albumId);

            // Assert
            album.Should().Be(null);
        }
    }
}
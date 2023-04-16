using System;
using System.Threading.Tasks;
using Chinook.DataCmpldQry;
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
        public void AlbumGetOne()
        {
            // Arrange
            var albumId = 1;
            var artistId = 1;

            // We are currently required to care about an Artist ID because the convert part of album specifically references the artist repository as well.
            _context.Artists.Add(new Artist() { Id = artistId, Name = "Artist"});
            _context.Albums.Add(new Album() { Id = albumId, Title = "Title", ArtistId = artistId });
            _context.SaveChanges();

            // Act
            var album = _repo.GetById(albumId);

            // Assert
            album.Id.Should().Be(albumId);
        }
        
        [Fact]
        public void AlbumAddOne()
        {
            // Arrange
            var id = 1;

            // Act
            var album = _repo.GetById(id);

            // Assert
            Assert.Equal(1, album.Id);
        }
        
        [Fact]
        public void AlbumUpdateOne()
        {
            // Arrange
            var id = 1;

            // Act
            var album = _repo.GetById(id);

            // Assert
            Assert.Equal(1, album.Id);
        }
        
        [Fact]
        public void AlbumDeleteOne()
        {
            // Arrange
            var id = 1;

            // Act
            var album = _repo.GetById(id);

            // Assert
            Assert.Equal(id, album.Id);
        }
    }
}
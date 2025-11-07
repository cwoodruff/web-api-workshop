using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<AlbumApiModel>> GetAllAlbum()
    {
        var key = _vCache.All("album");
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Album> albums = await _albumRepository.GetAll();
            return albums.ConvertAll();
        }, options) ?? Array.Empty<AlbumApiModel>();
    }

    public async Task<AlbumApiModel?> GetAlbumById(int id)
    {
        var key = _vCache.EntityById("album", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1))
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var album = await _albumRepository.GetById(id);
            if (album == null) return null;
            var albumApiModel = album.Convert();
            var artist = await GetArtistById(album.ArtistId);
            if (artist != null)
                albumApiModel.ArtistName = artist.Name;
            albumApiModel.Tracks = (await GetTrackByAlbumId(id) ?? Array.Empty<TrackApiModel>()).ToList();
            return albumApiModel;
        }, options);
    }

    public async Task<IEnumerable<AlbumApiModel>> GetAlbumByArtistId(int id)
    {
        var key = _vCache.ByFk("album", "by-artist", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var albums = await _albumRepository.GetByArtistId(id);
            return albums.ConvertAll();
        }, options) ?? Array.Empty<AlbumApiModel>();
    }

    public async Task<AlbumApiModel> AddAlbum(AlbumApiModel newAlbumApiModel)
    {
        await _albumValidator.ValidateAndThrowAsync(newAlbumApiModel);

        var album = newAlbumApiModel.Convert();

        album = await _albumRepository.Add(album);
        newAlbumApiModel.Id = album.Id;

        // Invalidate album lists; next lookups will use new version
        _vCache.BumpVersion("album");
        return newAlbumApiModel;
    }

    public async Task<bool> UpdateAlbum(AlbumApiModel albumApiModel)
    {
        await _albumValidator.ValidateAndThrowAsync(albumApiModel);

        var album = await _albumRepository.GetById(albumApiModel.Id);

        if (album is null) return false;
        album.Id = albumApiModel.Id;
        album.Title = albumApiModel.Title;
        album.ArtistId = albumApiModel.ArtistId;

        var updated = await _albumRepository.Update(album);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("album", album.Id));
            _vCache.BumpVersion("album");
        }
        return updated;
    }

    public async Task<bool> DeleteAlbum(int id)
    {
        var deleted = await _albumRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("album", id));
            _vCache.BumpVersion("album");
        }
        return deleted;
    }
}
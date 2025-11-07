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
        List<Album> albums = await _albumRepository.GetAll();
        var albumApiModels = albums.ConvertAll();

        foreach (var album in albumApiModels)
        {
            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("Album-", album.Id), album, (TimeSpan)cacheEntryOptions);
        }

        return albumApiModels;
    }

    public async Task<AlbumApiModel?> GetAlbumById(int id)
    {
        var albumApiModelCached = _cache.Get<AlbumApiModel>(string.Concat("Album-", id));

        if (albumApiModelCached != null)
        {
            return albumApiModelCached;
        }
        else
        {
            var album = await _albumRepository.GetById(id);
            if (album == null) return null;
            var albumApiModel = album.Convert();
            var result = (_artistRepository.GetById(album.ArtistId)).Result;
            if (result != null)
                albumApiModel.ArtistName = result.Name;
            albumApiModel.Tracks = (await GetTrackByAlbumId(id) ?? Array.Empty<TrackApiModel>()).ToList();

            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("Album-", albumApiModel.Id), albumApiModel, (TimeSpan)cacheEntryOptions);

            return albumApiModel;
        }
    }

    public async Task<IEnumerable<AlbumApiModel>> GetAlbumByArtistId(int id)
    {
        var albums = await _albumRepository.GetByArtistId(id);
        return albums.ConvertAll();
    }

    public async Task<AlbumApiModel> AddAlbum(AlbumApiModel newAlbumApiModel)
    {
        await _albumValidator.ValidateAndThrowAsync(newAlbumApiModel);

        var album = newAlbumApiModel.Convert();

        album = await _albumRepository.Add(album);
        newAlbumApiModel.Id = album.Id;
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

        return await _albumRepository.Update(album);
    }

    public Task<bool> DeleteAlbum(int id)
        => _albumRepository.Delete(id);
}
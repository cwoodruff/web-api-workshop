using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<AlbumApiModel>> GetAllAlbum()
    {
        List<Album> albums = await _albumRepository.GetAll();
        var albumApiModels = albums.ConvertAll();

        return albumApiModels;
    }

    public async Task<AlbumApiModel?> GetAlbumById(int id)
    {
        var album = await _albumRepository.GetById(id);
        if (album == null) return null;
        var albumApiModel = album.Convert();
        var result = (_artistRepository.GetById(album.ArtistId)).Result;
        if (result != null)
            albumApiModel.ArtistName = result.Name;
        albumApiModel.Tracks = (await GetTrackByAlbumId(id) ?? Array.Empty<TrackApiModel>()).ToList();

        return albumApiModel;
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
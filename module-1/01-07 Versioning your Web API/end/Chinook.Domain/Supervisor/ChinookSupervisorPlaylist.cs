using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<PlaylistApiModel>> GetAllPlaylist()
    {
        var key = _vCache.All("playlist");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Playlist> playlists = await _playlistRepository.GetAll();
            return playlists.ConvertAll();
        }, options) ?? Array.Empty<PlaylistApiModel>();
    }

    public async Task<PlaylistApiModel> GetPlaylistById(int id)
    {
        var key = _vCache.EntityById("playlist", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(12))
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var playlist = await _playlistRepository.GetById(id);
            if (playlist == null) return null!;
            var playlistApiModel = playlist.Convert();
            // Intentionally do not cache playlist-track associations here
            playlistApiModel.Tracks = (await GetTrackByPlaylistId(playlistApiModel.Id)).ToList();
            return playlistApiModel;
        }, options)!;
    }

    public async Task<PlaylistApiModel> AddPlaylist(PlaylistApiModel newPlaylistApiModel)
    {
        await _playlistValidator.ValidateAndThrowAsync(newPlaylistApiModel);

        var playlist = newPlaylistApiModel.Convert();

        playlist = await _playlistRepository.Add(playlist);
        newPlaylistApiModel.Id = playlist.Id;

        _vCache.BumpVersion("playlist");
        return newPlaylistApiModel;
    }

    public async Task<bool> UpdatePlaylist(PlaylistApiModel playlistApiModel)
    {
        await _playlistValidator.ValidateAndThrowAsync(playlistApiModel);

        var playlist = await _playlistRepository.GetById(playlistApiModel.Id);

        if (playlist == null) return false;
        playlist.Id = playlistApiModel.Id;
        playlist.Name = playlistApiModel.Name ?? string.Empty;

        var updated = await _playlistRepository.Update(playlist);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("playlist", playlist.Id));
            _vCache.BumpVersion("playlist");
        }
        return updated;
    }

    public async Task<bool> DeletePlaylist(int id)
    {
        var deleted = await _playlistRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("playlist", id));
            _vCache.BumpVersion("playlist");
        }
        return deleted;
    }

    // public async Task<IEnumerable<PlaylistApiModel>> GetPlaylistByTrackId(int id)
    // {
    //     var playlists = await _playlistRepository.GetByTrackId(id);
    //     return playlists.ConvertAll();
    // }
}
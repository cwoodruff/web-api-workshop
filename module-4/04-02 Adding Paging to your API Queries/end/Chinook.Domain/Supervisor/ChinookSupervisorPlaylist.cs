using Chinook.Domain.ApiModels;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<PagedList<PlaylistApiModel>> GetAllPlaylist(int pageNumber, int pageSize)
    {
        var playlists = await _playlistRepository.GetAll(pageNumber, pageSize);
        var playlistApiModels = playlists.ConvertAll();

        foreach (var playList in playlistApiModels)
        {
            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("Playlist-", playList.Id), playList, (TimeSpan)cacheEntryOptions);
        }
        var newPagedList = new PagedList<PlaylistApiModel>(playlistApiModels.ToList(), playlists.TotalCount, playlists.CurrentPage, playlists.PageSize);
        return newPagedList;
    }

    public async Task<PlaylistApiModel> GetPlaylistById(int id)
    {
        var playListApiModelCached = _cache.Get<PlaylistApiModel>(string.Concat("Playlist-", id));

        if (playListApiModelCached != null)
        {
            return playListApiModelCached;
        }
        else
        {
            var playlist = await _playlistRepository.GetById(id);
            if (playlist == null) return null!;
            var playlistApiModel = playlist.Convert();
            //playlistApiModel.Tracks = (await GetTrackByMediaTypeId(playlistApiModel.Id)).ToList();

            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("Playlist-", playlistApiModel.Id), playlistApiModel,
                (TimeSpan)cacheEntryOptions);

            return playlistApiModel;
        }
    }

    public async Task<PlaylistApiModel> AddPlaylist(PlaylistApiModel newPlaylistApiModel)
    {
        await _playlistValidator.ValidateAndThrowAsync(newPlaylistApiModel);

        var playlist = newPlaylistApiModel.Convert();

        playlist = await _playlistRepository.Add(playlist);
        newPlaylistApiModel.Id = playlist.Id;
        return newPlaylistApiModel;
    }

    public async Task<bool> UpdatePlaylist(PlaylistApiModel playlistApiModel)
    {
        await _playlistValidator.ValidateAndThrowAsync(playlistApiModel);

        var playlist = await _playlistRepository.GetById(playlistApiModel.Id);

        if (playlist == null) return false;
        playlist.Id = playlistApiModel.Id;
        playlist.Name = playlistApiModel.Name ?? string.Empty;

        return await _playlistRepository.Update(playlist);
    }

    public Task<bool> DeletePlaylist(int id)
        => _playlistRepository.Delete(id);
}
using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<PlaylistApiModel>> GetAllPlaylist()
    {
        List<Playlist> playlists = await _playlistRepository.GetAll();
        var playlistApiModels = playlists.ConvertAll();

        return playlistApiModels;
    }

    public async Task<PlaylistApiModel> GetPlaylistById(int id)
    {
        var playlist = await _playlistRepository.GetById(id);
        if (playlist == null) return null!;
        var playlistApiModel = playlist.Convert();
        playlistApiModel.Tracks = (await GetTrackByMediaTypeId(playlistApiModel.Id)).ToList();

        return playlistApiModel;
    }

    public async Task<PlaylistApiModel> AddPlaylist(PlaylistApiModel newPlaylistApiModel)
    {
        var playlist = newPlaylistApiModel.Convert();

        playlist = await _playlistRepository.Add(playlist);
        newPlaylistApiModel.Id = playlist.Id;
        return newPlaylistApiModel;
    }

    public async Task<bool> UpdatePlaylist(PlaylistApiModel playlistApiModel)
    {
        var playlist = await _playlistRepository.GetById(playlistApiModel.Id);

        if (playlist == null) return false;
        playlist.Id = playlistApiModel.Id;
        playlist.Name = playlistApiModel.Name ?? string.Empty;

        return await _playlistRepository.Update(playlist);
    }

    public Task<bool> DeletePlaylist(int id)
        => _playlistRepository.Delete(id);
}
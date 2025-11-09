using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<TrackApiModel>> GetAllTrack()
    {
        var key = _vCache.All("track");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Track> tracks = await _trackRepository.GetAll();
            return tracks.ConvertAll();
        }, options) ?? Array.Empty<TrackApiModel>();
    }

    public async Task<TrackApiModel?> GetTrackById(int id)
    {
        var key = _vCache.EntityById("track", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(12))
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var track = await _trackRepository.GetById(id);
            if (track == null) return null;
            var trackApiModel = track.Convert();
            trackApiModel.Genre = await GetGenreById(trackApiModel.GenreId);
            trackApiModel.Album = await GetAlbumById(trackApiModel.AlbumId);
            trackApiModel.MediaType = await GetMediaTypeById(trackApiModel.MediaTypeId);
            if (trackApiModel.Album != null) trackApiModel.AlbumName = trackApiModel.Album.Title;
            if (trackApiModel.MediaType != null) trackApiModel.MediaTypeName = trackApiModel.MediaType.Name;
            if (trackApiModel.Genre != null) trackApiModel.GenreName = trackApiModel.Genre.Name;
            return trackApiModel;
        }, options);
    }

    public async Task<IEnumerable<TrackApiModel>?> GetTrackByAlbumId(int id)
    {
        var key = _vCache.ByFk("track", "by-album", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var tracks = await _trackRepository.GetByAlbumId(id);
            return tracks?.ConvertAll();
        }, options);
    }

    public async Task<IEnumerable<TrackApiModel>> GetTrackByGenreId(int id)
    {
        var key = _vCache.ByFk("track", "by-genre", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var tracks = await _trackRepository.GetByGenreId(id);
            return tracks.ConvertAll();
        }, options) ?? Array.Empty<TrackApiModel>();
    }

    public async Task<IEnumerable<TrackApiModel>> GetTrackByMediaTypeId(int id)
    {
        var key = _vCache.ByFk("track", "by-media", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var tracks = await _trackRepository.GetByMediaTypeId(id);
            return tracks.ConvertAll();
        }, options) ?? Array.Empty<TrackApiModel>();
    }

    public async Task<IEnumerable<TrackApiModel>> GetTrackByPlaylistId(int id)
    {
        var key = _vCache.ByFk("track", "by-playlist", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var tracks = await _trackRepository.GetByPlaylistId(id);
            return tracks.ConvertAll();
        }, options) ?? Array.Empty<TrackApiModel>();
    }

    public async Task<TrackApiModel> AddTrack(TrackApiModel newTrackApiModel)
    {
        await _trackValidator.ValidateAndThrowAsync(newTrackApiModel);

        var track = newTrackApiModel.Convert();

        await _trackRepository.Add(track);
        newTrackApiModel.Id = track.Id;

        _vCache.BumpVersion("track");
        return newTrackApiModel;
    }

    public async Task<bool> UpdateTrack(TrackApiModel trackApiModel)
    {
        await _trackValidator.ValidateAndThrowAsync(trackApiModel);

        var track = await _trackRepository.GetById(trackApiModel.Id);

        if (track == null) return false;
        track.Id = trackApiModel.Id;
        track.Name = trackApiModel.Name;
        track.AlbumId = trackApiModel.AlbumId;
        track.MediaTypeId = trackApiModel.MediaTypeId;
        track.GenreId = trackApiModel.GenreId;
        track.Composer = trackApiModel.Composer ?? string.Empty;
        track.Milliseconds = trackApiModel.Milliseconds;
        track.Bytes = trackApiModel.Bytes;
        track.UnitPrice = trackApiModel.UnitPrice;

        var updated = await _trackRepository.Update(track);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("track", track.Id));
            _vCache.BumpVersion("track");
        }
        return updated;
    }

    public async Task<bool> DeleteTrack(int id)
    {
        var deleted = await _trackRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("track", id));
            _vCache.BumpVersion("track");
        }
        return deleted;
    }

    public async Task<IEnumerable<TrackApiModel>> GetTrackByArtistId(int id)
    {
        var key = _vCache.ByFk("track", "by-artist", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var tracks = await _trackRepository.GetByArtistId(id);
            return tracks.ConvertAll();
        }, options) ?? Array.Empty<TrackApiModel>();
    }

    public async Task<IEnumerable<TrackApiModel>> GetTrackByInvoiceId(int id)
    {
        var key = _vCache.ByFk("track", "by-invoice", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var tracks = await _trackRepository.GetByInvoiceId(id);
            return tracks.ConvertAll();
        }, options) ?? Array.Empty<TrackApiModel>();
    }
}
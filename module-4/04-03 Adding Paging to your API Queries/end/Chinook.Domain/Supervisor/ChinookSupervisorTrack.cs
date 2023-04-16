using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Chinook.Domain.ApiModels;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<PagedList<TrackApiModel>> GetAllTrack(int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetAll(pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();

        foreach (var track in trackApiModels)
        {
            DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions();
            cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(3600));
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(86400);

            await _distributedCache.SetStringAsync($"Track-{track.Id}", JsonSerializer.Serialize(track),
                cacheEntryOptions);
        }
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }

    public async Task<TrackApiModel?> GetTrackById(int id)
    {
        var trackApiModelCached = await _distributedCache.GetStringAsync($"Track-{id}");

        if (trackApiModelCached != null)
        {
            return JsonSerializer.Deserialize<TrackApiModel>(trackApiModelCached);
        }
        else
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

            DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions();
            cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(3600));
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(86400);

            await _distributedCache.SetStringAsync($"Track-{track.Id}", JsonSerializer.Serialize(trackApiModel),
                cacheEntryOptions);

            return trackApiModel;
        }
    }

    public async Task<TrackApiModel> AddTrack(TrackApiModel newTrackApiModel)
    {
        await _trackValidator.ValidateAndThrowAsync(newTrackApiModel);

        var track = newTrackApiModel.Convert();

        await _trackRepository.Add(track);
        newTrackApiModel.Id = track.Id;
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

        return await _trackRepository.Update(track);
    }

    public Task<bool> DeleteTrack(int id)
        => _trackRepository.Delete(id);
    
    public async Task<PagedList<TrackApiModel>?> GetTrackByAlbumId(int id, int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetByAlbumId(id, pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }

    public async Task<PagedList<TrackApiModel>> GetTrackByGenreId(int id, int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetByGenreId(id, pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }

    public async Task<PagedList<TrackApiModel>> GetTrackByMediaTypeId(int id, int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetByMediaTypeId(id, pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }

    public async Task<PagedList<TrackApiModel>> GetTrackByPlaylistId(int id, int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetByPlaylistId(id, pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }

    public async Task<PagedList<TrackApiModel>> GetTrackByArtistId(int id, int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetByArtistId(id, pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }

    public async Task<PagedList<TrackApiModel>> GetTrackByInvoiceId(int id, int pageNumber, int pageSize)
    {
        var tracks = await _trackRepository.GetByInvoiceId(id, pageNumber, pageSize);
        var trackApiModels = tracks.ConvertAll();
        var newPagedList = new PagedList<TrackApiModel>(trackApiModels.ToList(), tracks.TotalCount, tracks.CurrentPage, tracks.PageSize);
        return newPagedList;
    }
}
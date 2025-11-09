using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<ArtistApiModel>> GetAllArtist()
    {
        var key = _vCache.All("artist");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Artist> artists = await _artistRepository.GetAll();
            return artists.ConvertAll();
        }, options) ?? Array.Empty<ArtistApiModel>();
    }

    public async Task<ArtistApiModel> GetArtistById(int id)
    {
        var key = _vCache.EntityById("artist", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1))
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var artist = await _artistRepository.GetById(id);
            if (artist == null) return null!;
            var artistApiModel = artist.Convert();
            artistApiModel.Albums = (await _albumRepository.GetByArtistId(artist.Id)).ConvertAll().ToList();
            return artistApiModel;
        }, options)!;
    }

    public async Task<ArtistApiModel> AddArtist(ArtistApiModel newArtistApiModel)
    {
        await _artistValidator.ValidateAndThrowAsync(newArtistApiModel);

        var artist = newArtistApiModel.Convert();

        artist = await _artistRepository.Add(artist);
        newArtistApiModel.Id = artist.Id;

        _vCache.BumpVersion("artist");
        // Albums list may also be impacted (e.g., artist name shown on album views)
        _vCache.BumpVersion("album");
        return newArtistApiModel;
    }

    public async Task<bool> UpdateArtist(ArtistApiModel artistApiModel)
    {
        await _artistValidator.ValidateAndThrowAsync(artistApiModel);

        var artist = await _artistRepository.GetById(artistApiModel.Id);

        if (artist == null) return false;
        artist.Id = artistApiModel.Id;
        artist.Name = artistApiModel.Name ?? string.Empty;

        var updated = await _artistRepository.Update(artist);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("artist", artist.Id));
            _vCache.BumpVersion("artist");
            _vCache.BumpVersion("album");
        }
        return updated;
    }

    public async Task<bool> DeleteArtist(int id)
    {
        var deleted = await _artistRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("artist", id));
            _vCache.BumpVersion("artist");
            _vCache.BumpVersion("album");
        }
        return deleted;
    }
}
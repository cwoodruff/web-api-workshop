using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<MediaTypeApiModel>> GetAllMediaType()
    {
        var key = _vCache.All("mediatype");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(12));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<MediaType> mediaTypes = await _mediaTypeRepository.GetAll();
            return mediaTypes.ConvertAll();
        }, options) ?? Array.Empty<MediaTypeApiModel>();
    }

    public async Task<MediaTypeApiModel?> GetMediaTypeById(int id)
    {
        var key = _vCache.EntityById("mediatype", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(2))
            .SetSlidingExpiration(TimeSpan.FromHours(6));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var mediaType = await _mediaTypeRepository.GetById(id);
            if (mediaType == null) return null;
            var mediaTypeApiModel = mediaType.Convert();
            mediaTypeApiModel.Tracks = (await GetTrackByMediaTypeId(mediaTypeApiModel.Id)).ToList();
            return mediaTypeApiModel;
        }, options);
    }

    public async Task<MediaTypeApiModel> AddMediaType(MediaTypeApiModel newMediaTypeApiModel)
    {
        await _mediaTypeValidator.ValidateAndThrowAsync(newMediaTypeApiModel);

        var mediaType = newMediaTypeApiModel.Convert();

        mediaType = await _mediaTypeRepository.Add(mediaType);
        newMediaTypeApiModel.Id = mediaType.Id;

        _vCache.BumpVersion("mediatype");
        _vCache.BumpVersion("track");
        return newMediaTypeApiModel;
    }

    public async Task<bool> UpdateMediaType(MediaTypeApiModel mediaTypeApiModel)
    {
        await _mediaTypeValidator.ValidateAndThrowAsync(mediaTypeApiModel);

        var mediaType = await _mediaTypeRepository.GetById(mediaTypeApiModel.Id);

        if (mediaType == null) return false;
        mediaType.Id = mediaTypeApiModel.Id;
        mediaType.Name = mediaTypeApiModel.Name ?? string.Empty;

        var updated = await _mediaTypeRepository.Update(mediaType);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("mediatype", mediaType.Id));
            _vCache.BumpVersion("mediatype");
            _vCache.BumpVersion("track");
        }
        return updated;
    }

    public async Task<bool> DeleteMediaType(int id)
    {
        var deleted = await _mediaTypeRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("mediatype", id));
            _vCache.BumpVersion("mediatype");
            _vCache.BumpVersion("track");
        }
        return deleted;
    }
}
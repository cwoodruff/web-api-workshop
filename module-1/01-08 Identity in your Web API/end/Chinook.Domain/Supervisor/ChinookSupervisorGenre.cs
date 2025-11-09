using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<GenreApiModel>> GetAllGenre()
    {
        var key = _vCache.All("genre");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(12));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Genre> genres = await _genreRepository.GetAll();
            return genres.ConvertAll();
        }, options) ?? Array.Empty<GenreApiModel>();
    }

    public async Task<GenreApiModel?> GetGenreById(int id)
    {
        var key = _vCache.EntityById("genre", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(2))
            .SetSlidingExpiration(TimeSpan.FromHours(6));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var genre = await _genreRepository.GetById(id);
            if (genre == null) return null;
            var genreApiModel = genre.Convert();
            genreApiModel.Tracks = (await GetTrackByGenreId(genreApiModel.Id)).ToList();
            return genreApiModel;
        }, options);
    }

    public async Task<GenreApiModel> AddGenre(GenreApiModel newGenreApiModel)
    {
        await _genreValidator.ValidateAndThrowAsync(newGenreApiModel);

        var genre = newGenreApiModel.Convert();

        genre = await _genreRepository.Add(genre);
        newGenreApiModel.Id = genre.Id;

        _vCache.BumpVersion("genre");
        _vCache.BumpVersion("track");
        return newGenreApiModel;
    }

    public async Task<bool> UpdateGenre(GenreApiModel genreApiModel)
    {
        await _genreValidator.ValidateAndThrowAsync(genreApiModel);

        var genre = await _genreRepository.GetById(genreApiModel.Id);

        if (genre == null) return false;
        genre.Id = genreApiModel.Id;
        genre.Name = genreApiModel.Name ?? string.Empty;

        var updated = await _genreRepository.Update(genre);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("genre", genre.Id));
            _vCache.BumpVersion("genre");
            _vCache.BumpVersion("track");
        }
        return updated;
    }

    public async Task<bool> DeleteGenre(int id)
    {
        var deleted = await _genreRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("genre", id));
            _vCache.BumpVersion("genre");
            _vCache.BumpVersion("track");
        }
        return deleted;
    }
}
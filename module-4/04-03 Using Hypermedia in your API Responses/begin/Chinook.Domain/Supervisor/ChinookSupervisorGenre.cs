using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<GenreApiModel>> GetAllGenre()
    {
        List<Genre> genres = await _genreRepository.GetAll();
        var genreApiModels = genres.ConvertAll();

        return genreApiModels;
    }

    public async Task<GenreApiModel?> GetGenreById(int id)
    {
        var genre = await _genreRepository.GetById(id);
        if (genre == null) return null;
        var genreApiModel = genre.Convert();
        genreApiModel.Tracks = (await GetTrackByGenreId(genreApiModel.Id)).ToList();

        return genreApiModel;
    }

    public async Task<GenreApiModel> AddGenre(GenreApiModel newGenreApiModel)
    {
        await _genreValidator.ValidateAndThrowAsync(newGenreApiModel);

        var genre = newGenreApiModel.Convert();

        genre = await _genreRepository.Add(genre);
        newGenreApiModel.Id = genre.Id;
        return newGenreApiModel;
    }

    public async Task<bool> UpdateGenre(GenreApiModel genreApiModel)
    {
        await _genreValidator.ValidateAndThrowAsync(genreApiModel);

        var genre = await _genreRepository.GetById(genreApiModel.Id);

        if (genre == null) return false;
        genre.Id = genreApiModel.Id;
        genre.Name = genreApiModel.Name ?? string.Empty;

        return await _genreRepository.Update(genre);
    }

    public Task<bool> DeleteGenre(int id)
        => _genreRepository.Delete(id);
}
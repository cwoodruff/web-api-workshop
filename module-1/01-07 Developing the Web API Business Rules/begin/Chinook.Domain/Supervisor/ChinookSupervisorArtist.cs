using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<ArtistApiModel>> GetAllArtist()
    {
        List<Artist> artists = await _artistRepository.GetAll();
        var artistApiModels = artists.ConvertAll();

        return artistApiModels;
    }

    public async Task<ArtistApiModel> GetArtistById(int id)
    {
        var artist = await _artistRepository.GetById(id);
        if (artist == null) return null!;
        var artistApiModel = artist.Convert();
        artistApiModel.Albums = (await _albumRepository.GetByArtistId(artist.Id)).ConvertAll().ToList();

        return artistApiModel;
    }

    public async Task<ArtistApiModel> AddArtist(ArtistApiModel newArtistApiModel)
    {
        var artist = newArtistApiModel.Convert();

        artist = await _artistRepository.Add(artist);
        newArtistApiModel.Id = artist.Id;
        return newArtistApiModel;
    }

    public async Task<bool> UpdateArtist(ArtistApiModel artistApiModel)
    {
        var artist = await _artistRepository.GetById(artistApiModel.Id);

        if (artist == null) return false;
        artist.Id = artistApiModel.Id;
        artist.Name = artistApiModel.Name ?? string.Empty;

        return await _artistRepository.Update(artist);
    }

    public Task<bool> DeleteArtist(int id)
        => _artistRepository.Delete(id);
}
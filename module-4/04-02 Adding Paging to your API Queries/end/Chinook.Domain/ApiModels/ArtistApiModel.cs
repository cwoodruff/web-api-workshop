using Chinook.Domain.Converters;
using Chinook.Domain.Entities;

namespace Chinook.Domain.ApiModels;

public class ArtistApiModel : IConvertModel<Artist>
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public IList<AlbumApiModel>? Albums { get; set; }

    public Artist Convert() =>
        new()
        {
            Id = Id,
            Name = Name ?? string.Empty
        };
}
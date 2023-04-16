using Chinook.Domain.ApiModels;
using Chinook.Domain.Converters;

namespace Chinook.Domain.Entities;

public sealed class Artist : IConvertModel<ArtistApiModel>
{
    public Artist()
    {
        Albums = new HashSet<Album>();
    }

    public int Id { get; set; }
    public string? Name { get; set; }

    public ICollection<Album>? Albums { get; set; }

    public ArtistApiModel Convert() =>
        new()
        {
            Id = Id,
            Name = Name
        };
}
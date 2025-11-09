using Chinook.Domain.ApiModels;
using Chinook.Domain.Converters;

namespace Chinook.Domain.Entities;

public sealed class MediaType : IConvertModel<MediaTypeApiModel>
{
    public MediaType()
    {
        Tracks = new HashSet<Track>();
    }

    public int Id { get; set; }
    public string? Name { get; set; }

    public ICollection<Track>? Tracks { get; set; }

    public MediaTypeApiModel Convert() =>
        new()
        {
            Id = Id,
            Name = Name
        };
}
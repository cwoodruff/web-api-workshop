using Chinook.Domain.ApiModels;
using Chinook.Domain.Converters;

namespace Chinook.Domain.Entities;

public partial class Album : IConvertModel<AlbumApiModel>
{
    public Album()
    {
        Tracks = new HashSet<Track>();
    }

    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ArtistId { get; set; }

    public virtual Artist Artist { get; set; } = null!;
    public virtual ICollection<Track> Tracks { get; set; }

    public AlbumApiModel Convert() =>
        new()
        {
            Id = Id,
            ArtistId = ArtistId,
            Title = Title
        };
}
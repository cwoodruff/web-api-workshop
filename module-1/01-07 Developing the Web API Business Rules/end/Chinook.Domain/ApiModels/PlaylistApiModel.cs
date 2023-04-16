using Chinook.Domain.Converters;
using Chinook.Domain.Entities;

namespace Chinook.Domain.ApiModels;

public class PlaylistApiModel : IConvertModel<Playlist>
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public IList<TrackApiModel>? Tracks { get; set; }

    public Playlist Convert() =>
        new()
        {
            Id = Id,
            Name = Name
        };
}
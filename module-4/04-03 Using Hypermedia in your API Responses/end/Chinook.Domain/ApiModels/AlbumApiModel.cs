using Chinook.Domain.Converters;
using Chinook.Domain.Entities;

namespace Chinook.Domain.ApiModels;

public class AlbumApiModel : BaseApiModel, IConvertModel<Album>
{
    public string Title { get; set; }
    public int ArtistId { get; set; }
    public string ArtistName { get; set; }

    public ArtistApiModel Artist { get; set; }

    public IList<TrackApiModel> Tracks { get; set; }

    public Album Convert() =>
        new()
        {
            Id = Id,
            ArtistId = ArtistId,
            Title = Title ?? string.Empty
        };
}
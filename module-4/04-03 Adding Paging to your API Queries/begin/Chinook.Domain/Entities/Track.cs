using Chinook.Domain.ApiModels;
using Chinook.Domain.Converters;

namespace Chinook.Domain.Entities;

public class Track : IConvertModel<TrackApiModel>
{
    public Track()
    {
        InvoiceLines = new HashSet<InvoiceLine>();
    }

    public int Id { get; set; }
    public string? Name { get; set; }
    public int AlbumId { get; set; }
    public int MediaTypeId { get; set; }
    public int GenreId { get; set; }
    public string? Composer { get; set; }
    public int Milliseconds { get; set; }
    public int Bytes { get; set; }
    public decimal UnitPrice { get; set; }
    public virtual Album? Album { get; set; }
    public virtual Genre? Genre { get; set; }
    public virtual MediaType? MediaType { get; set; }
    public virtual ICollection<InvoiceLine>? InvoiceLines { get; set; }


    public virtual ICollection<Playlist>? Playlists { get; set; }

    public TrackApiModel Convert() =>
        new()
        {
            Id = Id,
            Name = Name,
            AlbumId = AlbumId,
            MediaTypeId = MediaTypeId,
            GenreId = GenreId,
            Composer = Composer,
            Milliseconds = Milliseconds,
            Bytes = Bytes,
            UnitPrice = UnitPrice
        };
}
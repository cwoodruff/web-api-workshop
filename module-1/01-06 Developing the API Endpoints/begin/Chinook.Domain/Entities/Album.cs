namespace Chinook.Domain.Entities;

public partial class Album
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
}
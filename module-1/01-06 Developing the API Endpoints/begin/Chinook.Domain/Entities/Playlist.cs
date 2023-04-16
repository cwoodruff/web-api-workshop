namespace Chinook.Domain.Entities;

public partial class Playlist
{
    public Playlist()
    {
        Tracks = new HashSet<Track>();
    }

    public int Id { get; set; }
    public string? Name { get; set; }

    public virtual ICollection<Track> Tracks { get; set; }
}
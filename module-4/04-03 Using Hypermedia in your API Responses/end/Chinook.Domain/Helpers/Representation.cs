namespace Chinook.Domain.Helpers;

public interface IRepresentation
{
    List<Link> Links { get; }
    void AddLink(Link link);
}

public abstract class Representation : IRepresentation
{
    public List<Link> Links { get; } = new();

    public void AddLink(Link link)
    {
        var existing = Links.FindIndex(l => string.Equals(l.Rel, link.Rel, StringComparison.OrdinalIgnoreCase));
        if (existing >= 0)
            Links[existing] = link;
        else
            Links.Add(link);
    }
}

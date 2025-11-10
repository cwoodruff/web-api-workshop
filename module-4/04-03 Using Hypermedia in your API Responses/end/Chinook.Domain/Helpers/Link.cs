namespace Chinook.Domain.Helpers;

public sealed class Link
{
    public string Rel { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string? Title { get; set; }
    // Optional affordances for future use
    public string? Method { get; set; }
    public string? Type { get; set; }
}

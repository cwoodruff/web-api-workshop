using Chinook.Domain.ApiModels;
using Chinook.Domain.Helpers;
using Chinook.Domain.Hypermedia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chinook.API.Hypermedia.Enrichers;

public sealed class AlbumEnricher : Enricher<AlbumApiModel>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public AlbumEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
    }

    protected override Task ProcessAsync(AlbumApiModel model, CancellationToken ct)
    {
        var http = _httpContextAccessor.HttpContext!;

        var self = _linkGenerator.GetUriByName(http, "GetAlbumById", new { id = model.Id });
        if (!string.IsNullOrEmpty(self))
        {
            model.AddLink(new Link { Rel = "self", Title = $"Album #{model.Id}", Href = self });
        }

        if (model.ArtistId != 0)
        {
            var artistHref = _linkGenerator.GetUriByName(http, "GetArtistById", new { id = model.ArtistId });
            if (!string.IsNullOrEmpty(artistHref))
            {
                model.AddLink(new Link { Rel = "artist", Title = $"Artist #{model.ArtistId}", Href = artistHref });
            }
        }

        return Task.CompletedTask;
    }
}

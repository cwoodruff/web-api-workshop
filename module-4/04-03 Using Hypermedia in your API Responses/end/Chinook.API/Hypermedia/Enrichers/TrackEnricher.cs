using Chinook.Domain.ApiModels;
using Chinook.Domain.Helpers;
using Chinook.Domain.Hypermedia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chinook.API.Hypermedia.Enrichers;

public sealed class TrackEnricher : Enricher<TrackApiModel>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public TrackEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
    }

    protected override Task ProcessAsync(TrackApiModel model, CancellationToken ct)
    {
        var http = _httpContextAccessor.HttpContext!;

        var self = _linkGenerator.GetUriByName(http, "GetTrackById", new { id = model.Id });
        if (!string.IsNullOrEmpty(self))
        {
            model.AddLink(new Link { Rel = "self", Title = $"Track #{model.Id}", Href = self });
        }

        if (model.AlbumId is int albumId)
        {
            var albumHref = _linkGenerator.GetUriByName(http, "GetAlbumById", new { id = albumId });
            if (!string.IsNullOrEmpty(albumHref))
            {
                model.AddLink(new Link { Rel = "album", Title = $"Album #{albumId}", Href = albumHref });
            }
        }

        if (model.GenreId is int genreId)
        {
            var genreHref = _linkGenerator.GetUriByName(http, "GetGenreById", new { id = genreId });
            if (!string.IsNullOrEmpty(genreHref))
            {
                model.AddLink(new Link { Rel = "genre", Title = $"Genre #{genreId}", Href = genreHref });
            }
        }

        if (model.MediaTypeId is int mediaTypeId)
        {
            var mediaHref = _linkGenerator.GetUriByName(http, "GetMediaTypeById", new { id = mediaTypeId });
            if (!string.IsNullOrEmpty(mediaHref))
            {
                model.AddLink(new Link { Rel = "mediaType", Title = $"Media Type #{mediaTypeId}", Href = mediaHref });
            }
        }

        return Task.CompletedTask;
    }
}

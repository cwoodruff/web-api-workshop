using Chinook.Domain.ApiModels;
using Chinook.Domain.Helpers;
using Chinook.Domain.Hypermedia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chinook.API.Hypermedia.Enrichers;

public sealed class AlbumsEnricher : ListEnricher<List<AlbumApiModel>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public AlbumsEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
    }

    protected override Task ProcessAsync(List<AlbumApiModel> models, CancellationToken ct)
    {
        var http = _httpContextAccessor.HttpContext!;

        foreach (var model in models)
        {
            var self = _linkGenerator.GetUriByName(http, "GetAlbumById", new { id = model.Id });
            if (!string.IsNullOrEmpty(self))
            {
                model.AddLink(new Link { Rel = "self", Title = $"Album #{model.Id}", Href = self });
            }
        }

        return Task.CompletedTask;
    }
}

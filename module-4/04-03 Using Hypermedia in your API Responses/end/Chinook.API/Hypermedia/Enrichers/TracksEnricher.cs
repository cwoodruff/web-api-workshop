using Chinook.Domain.ApiModels;
using Chinook.Domain.Helpers;
using Chinook.Domain.Hypermedia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chinook.API.Hypermedia.Enrichers;

public sealed class TracksEnricher : ListEnricher<List<TrackApiModel>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public TracksEnricher(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
    }

    protected override Task ProcessAsync(List<TrackApiModel> models, CancellationToken ct)
    {
        var http = _httpContextAccessor.HttpContext!;

        foreach (var model in models)
        {
            var self = _linkGenerator.GetUriByName(http, "GetTrackById", new { id = model.Id });
            if (!string.IsNullOrEmpty(self))
            {
                model.AddLink(new Link { Rel = "self", Title = $"Track #{model.Id}", Href = self });
            }
        }

        return Task.CompletedTask;
    }
}

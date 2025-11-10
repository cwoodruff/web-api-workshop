using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chinook.Domain.Hypermedia;

public sealed class RepresentationEnricher : IAsyncResultFilter
{
    private readonly IEnumerable<IEnricher> _enrichers;

    public RepresentationEnricher(IEnumerable<IEnricher> enrichers)
    {
        _enrichers = enrichers;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            var value = objectResult.Value;
            foreach (var enricher in _enrichers)
            {
                if (enricher.Match(value))
                {
                    await enricher.ProcessAsync(value, context.HttpContext.RequestAborted);
                }
            }
        }

        await next();
    }
}

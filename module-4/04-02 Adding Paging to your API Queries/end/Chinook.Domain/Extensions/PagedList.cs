using Microsoft.EntityFrameworkCore;

namespace Chinook.Domain.Extensions;

public class PagedList<T> : List<T>
{
    public int CurrentPage { get; }
    public int TotalPages { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }
    
    public PagedList(PagedList<T> items)
    {
        TotalCount = items.TotalCount;
        PageSize = items.PageSize;
        CurrentPage = items.CurrentPage;
        TotalPages = (int)Math.Ceiling(items.TotalCount / (double)items.PageSize);
        AddRange(items);
    }
    
    public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IGenreRepository : IDisposable
{
    Task<PagedList<Genre>> GetAll(int pageNumber, int pageSize);
    Task<Genre> GetById(int id);
    Task<Genre> Add(Genre newGenre);
    Task<bool> Update(Genre genre);
    Task<bool> Delete(int id);
}
using Chinook.Domain.Entities;

namespace Chinook.Domain.Repositories;

public interface IGenreRepository : IDisposable
{
    Task<List<Genre>> GetAll();
    Task<Genre> GetById(int id);
    Task<Genre> Add(Genre newGenre);
    Task<bool> Update(Genre genre);
    Task<bool> Delete(int id);
}
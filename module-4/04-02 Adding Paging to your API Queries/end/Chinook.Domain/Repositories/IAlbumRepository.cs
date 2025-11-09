using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IAlbumRepository : IDisposable
{
    Task<PagedList<Album>> GetAll(int pageNumber, int pageSize);
    Task<Album> GetById(int id);
    Task<PagedList<Album>> GetByArtistId(int id, int pageNumber, int pageSize);
    Task<Album> Add(Album newAlbum);
    Task<bool> Update(Album album);
    Task<bool> Delete(int id);
}
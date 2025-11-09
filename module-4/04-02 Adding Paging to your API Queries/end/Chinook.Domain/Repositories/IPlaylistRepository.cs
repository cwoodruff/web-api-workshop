using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IPlaylistRepository : IDisposable
{
    Task<PagedList<Playlist>> GetAll(int pageNumber, int pageSize);
    Task<Playlist> GetById(int id);
    Task<Playlist> Add(Playlist newPlaylist);
    Task<bool> Update(Playlist playlist);
    Task<bool> Delete(int id);
    Task<PagedList<Playlist>> GetByTrackId(int id, int pageNumber, int pageSize);
}
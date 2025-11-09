using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly ChinookContext _context;

    public PlaylistRepository(ChinookContext context)
    {
        _context = context;
    }

    private async Task<bool> PlaylistExists(int id) =>
        await _context.Playlists.AnyAsync(i => i.Id == id);

    public void Dispose() => _context.Dispose();

    public async Task<PagedList<Playlist>> GetAll(int pageNumber, int pageSize) =>
        await PagedList<Playlist>.ToPagedListAsync(_context.Set<Playlist>().AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<Playlist> GetById(int id) =>
        await _context.Playlists.FindAsync(id);

    public async Task<Playlist> Add(Playlist newPlaylist)
    {
        await _context.Playlists.AddAsync(newPlaylist);
        await _context.SaveChangesAsync();
        return newPlaylist;
    }

    public async Task<bool> Update(Playlist playlist)
    {
        if (!await PlaylistExists(playlist.Id))
            return false;
        _context.Playlists.Update(playlist);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await PlaylistExists(id))
            return false;
        var toRemove = await _context.Playlists.FindAsync(id);
        _context.Playlists.Remove(toRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedList<Playlist>> GetByTrackId(int id, int pageNumber, int pageSize) =>
        await PagedList<Playlist>.ToPagedListAsync(_context.Playlists.Where(c => c.Tracks.Any(o => o.Id == id))
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
}
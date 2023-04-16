using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data.Repositories;

public class TrackRepository : ITrackRepository
{
    private readonly ChinookContext _context;

    public TrackRepository(ChinookContext context)
    {
        _context = context;
    }

    private async Task<bool> TrackExists(int id) =>
        await _context.Tracks.AnyAsync(i => i.Id == id);

    public void Dispose() => _context.Dispose();

    public async Task<PagedList<Track>> GetAll(int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Set<Track>().AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<Track> GetById(int id) =>
        await _context.Tracks.FindAsync(id);

    public async Task<Track> Add(Track newTrack)
    {
        await _context.Tracks.AddAsync(newTrack);
        await _context.SaveChangesAsync();
        return newTrack;
    }

    public async Task<bool> Update(Track track)
    {
        if (!await TrackExists(track.Id))
            return false;
        _context.Tracks.Update(track);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await TrackExists(id))
            return false;
        var toRemove = await _context.Tracks.FindAsync(id);
        _context.Tracks.Remove(toRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedList<Track>?> GetByAlbumId(int id, int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Tracks.Where(a => a.AlbumId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
    
    public async Task<PagedList<Track>> GetByGenreId(int id, int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Tracks.Where(a => a.GenreId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
    
    public async Task<PagedList<Track>> GetByMediaTypeId(int id, int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Tracks.Where(a => a.MediaTypeId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
    
    public async Task<PagedList<Track>> GetByPlaylistId(int id, int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Playlists.Where(p => p.Id == id).SelectMany(p => p.Tracks)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
    
    public async Task<PagedList<Track>> GetByArtistId(int id, int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Albums.Where(a => a.ArtistId == id).SelectMany(t => t.Tracks)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
    
    public async Task<PagedList<Track>> GetByInvoiceId(int id, int pageNumber, int pageSize) =>
        await PagedList<Track>.ToPagedListAsync(_context.Tracks.Where(c => c.InvoiceLines.Any(o => o.InvoiceId == id))
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
}
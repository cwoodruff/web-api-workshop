using Chinook.DataCmpldQry;
using Chinook.DataCmpldQry.Repositories;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.UnitTests.Helpers;

public class TestHelper
{
    private readonly ChinookContext _context;

    public ChinookContext Context => _context;

    public TestHelper()
    {
        var builder = new DbContextOptionsBuilder<ChinookContext>();
        builder.UseInMemoryDatabase(databaseName: "ChinookDbInMemory");
        builder.EnableSensitiveDataLogging();

        var dbContextOptions = builder.Options;
        _context = new ChinookContext(dbContextOptions);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public IAlbumRepository AlbumInMemoryRepository()
    {
        return new AlbumRepository(_context);
    }
    
    public IArtistRepository ArtistInMemoryRepository()
    {
        return new ArtistRepository(_context);
    }

    public ICustomerRepository CustomerInMemoryRepository()
    {
        return new CustomerRepository(_context);
    }
    
    public IEmployeeRepository EmployeeInMemoryRepository()
    {
        return new EmployeeRepository(_context);
    }
    
    public IGenreRepository GenreInMemoryRepository()
    {
        return new GenreRepository(_context);
    }
    
    public IInvoiceRepository InvoiceInMemoryRepository()
    {
        return new InvoiceRepository(_context);
    }
    
    public IInvoiceLineRepository InvoiceLineInMemoryRepository()
    {
        return new InvoiceLineRepository(_context);
    }
    
    public IMediaTypeRepository MediaTypeInMemoryRepository()
    {
        return new MediaTypeRepository(_context);
    }
    
    public IPlaylistRepository PlaylistInMemoryRepository()
    {
        return new PlaylistRepository(_context);
    }
    
    public ITrackRepository TrackInMemoryRepository()
    {
        return new TrackRepository(_context);
    }
}
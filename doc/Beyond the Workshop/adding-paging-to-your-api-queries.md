---
order: 3
icon: rocket
---
# Adding Paging to your API Queries

## START FROM PREVIOUS MODULE'S END
[Using the PATCH HTTP Request in your API](using-patch-request.md)

## Add Microsoft.EntityFrameworkCore NUGET PACKAGE TO DOMAIN PROJECT

```dos
dotnet add package Microsoft.EntityFrameworkCore
```

## ADD PAGEDLIST CLASS TO YOUR DOMAIN PROJECT

```csharp
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
```

## UPDATE DATA REPOSITORY INTERFACES IN DOMAIN PROJECT

```csharp
public interface IAlbumRepository : IDisposable
{
    Task<PagedList<Album>> GetAll(int pageNumber, int pageSize);
    Task<Album> GetById(int id);
    Task<PagedList<Album>> GetByArtistId(int id, int pageNumber, int pageSize);
    Task<Album> Add(Album newAlbum);
    Task<bool> Update(Album album);
    Task<bool> Delete(int id);
}
```

## UPDATE DATA REPOSITORIES IN DATA PROJECT

```csharp
public class AlbumRepository : IAlbumRepository
{
    private readonly ChinookContext _context;

    public AlbumRepository(ChinookContext context)
    {
        _context = context;
    }

    private async Task<bool> AlbumExists(int id) =>
        await _context.Albums.AnyAsync(a => a.Id == id);

    public void Dispose() => _context.Dispose();

    public async Task<PagedList<Album>> GetAll(int pageNumber, int pageSize) =>
        await PagedList<Album>.ToPagedListAsync(_context.Set<Album>().AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<Album> GetById(int id)
    {
        var dbAlbum = await _context.Albums.FindAsync(id);
        return dbAlbum;
    }

    public async Task<Album> Add(Album newAlbum)
    {
        await _context.Albums.AddAsync(newAlbum);
        await _context.SaveChangesAsync();
        return newAlbum;
    }

    public async Task<bool> Update(Album album)
    {
        if (!await AlbumExists(album.Id))
            return false;
        _context.Albums.Update(album);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await AlbumExists(id))
            return false;
        var toRemove = await _context.Albums.FindAsync(id);
        _context.Albums.Remove(toRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedList<Album>> GetByArtistId(int id, int pageNumber, int pageSize) =>
        await PagedList<Album>.ToPagedListAsync(_context.Albums.Where(a => a.ArtistId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
}
```

## UPDATE ICHINOOKSUPERVISOR IN DOMAIN PROJECT

```csharp
public interface IChinookSupervisor
{
    Task<PagedList<AlbumApiModel>> GetAllAlbum(int pageNumber, int pageSize);
    Task<AlbumApiModel?> GetAlbumById(int id);
    Task<PagedList<AlbumApiModel>> GetAlbumByArtistId(int id, int pageNumber, int pageSize);
    Task<AlbumApiModel> AddAlbum(AlbumApiModel newAlbumApiModel);
    Task<bool> UpdateAlbum(AlbumApiModel albumApiModel);
    Task<bool> DeleteAlbum(int id);
    Task<PagedList<ArtistApiModel>> GetAllArtist(int pageNumber, int pageSize);
    Task<ArtistApiModel> GetArtistById(int id);
    Task<ArtistApiModel> AddArtist(ArtistApiModel newArtistApiModel);
    Task<bool> UpdateArtist(ArtistApiModel artistApiModel);
    Task<bool> DeleteArtist(int id);
    Task<PagedList<CustomerApiModel>> GetAllCustomer(int pageNumber, int pageSize);
    Task<CustomerApiModel> GetCustomerById(int id);
    Task<PagedList<CustomerApiModel>> GetCustomerBySupportRepId(int id, int pageNumber, int pageSize);
    Task<CustomerApiModel> AddCustomer(CustomerApiModel newCustomerApiModel);
    Task<bool> UpdateCustomer(CustomerApiModel customerApiModel);
    Task<bool> DeleteCustomer(int id);
    Task<PagedList<EmployeeApiModel>> GetAllEmployee(int pageNumber, int pageSize);
    Task<EmployeeApiModel?> GetEmployeeById(int id);
    Task<EmployeeApiModel?> GetEmployeeReportsTo(int id);
    Task<EmployeeApiModel> AddEmployee(EmployeeApiModel newEmployeeApiModel);
    Task<bool> UpdateEmployee(EmployeeApiModel employeeApiModel);
    Task<bool> DeleteEmployee(int id);
    Task<IEnumerable<EmployeeApiModel>> GetEmployeeDirectReports(int id);
    Task<IEnumerable<EmployeeApiModel>> GetDirectReports(int id);
    Task<PagedList<GenreApiModel>> GetAllGenre(int pageNumber, int pageSize);
    Task<GenreApiModel?> GetGenreById(int id);
    Task<GenreApiModel> AddGenre(GenreApiModel newGenreApiModel);
    Task<bool> UpdateGenre(GenreApiModel genreApiModel);
    Task<bool> DeleteGenre(int id);
    Task<PagedList<InvoiceLineApiModel>> GetAllInvoiceLine(int pageNumber, int pageSize);
    Task<InvoiceLineApiModel> GetInvoiceLineById(int id);
    Task<PagedList<InvoiceLineApiModel>> GetInvoiceLineByInvoiceId(int id, int pageNumber, int pageSize);
    Task<PagedList<InvoiceLineApiModel>> GetInvoiceLineByTrackId(int id, int pageNumber, int pageSize);
    Task<InvoiceLineApiModel> AddInvoiceLine(InvoiceLineApiModel newInvoiceLineApiModel);
    Task<bool> UpdateInvoiceLine(InvoiceLineApiModel invoiceLineApiModel);
    Task<bool> DeleteInvoiceLine(int id);
    Task<PagedList<InvoiceApiModel>> GetAllInvoice(int pageNumber, int pageSize);
    Task<InvoiceApiModel?> GetInvoiceById(int id);
    Task<PagedList<InvoiceApiModel>> GetInvoiceByCustomerId(int id, int pageNumber, int pageSize);
    Task<InvoiceApiModel> AddInvoice(InvoiceApiModel newInvoiceApiModel);
    Task<bool> UpdateInvoice(InvoiceApiModel invoiceApiModel);
    Task<bool> DeleteInvoice(int id);
    Task<PagedList<InvoiceApiModel>> GetInvoiceByEmployeeId(int id, int pageNumber, int pageSize);
    Task<PagedList<MediaTypeApiModel>> GetAllMediaType(int pageNumber, int pageSize);
    Task<MediaTypeApiModel?> GetMediaTypeById(int id);
    Task<MediaTypeApiModel> AddMediaType(MediaTypeApiModel newMediaTypeApiModel);
    Task<bool> UpdateMediaType(MediaTypeApiModel mediaTypeApiModel);
    Task<bool> DeleteMediaType(int id);
    Task<PagedList<PlaylistApiModel>> GetAllPlaylist(int pageNumber, int pageSize);
    Task<PlaylistApiModel> GetPlaylistById(int id);
    Task<PlaylistApiModel> AddPlaylist(PlaylistApiModel newPlaylistApiModel);
    Task<bool> UpdatePlaylist(PlaylistApiModel playlistApiModel);
    Task<bool> DeletePlaylist(int id);
    Task<PagedList<TrackApiModel>> GetAllTrack(int pageNumber, int pageSize);
    Task<TrackApiModel?> GetTrackById(int id);
    Task<PagedList<TrackApiModel>?> GetTrackByAlbumId(int id, int pageNumber, int pageSize);
    Task<PagedList<TrackApiModel>> GetTrackByGenreId(int id, int pageNumber, int pageSize);
    Task<PagedList<TrackApiModel>> GetTrackByMediaTypeId(int id, int pageNumber, int pageSize);
    Task<PagedList<TrackApiModel>> GetTrackByPlaylistId(int id, int pageNumber, int pageSize);
    Task<TrackApiModel> AddTrack(TrackApiModel newTrackApiModel);
    Task<bool> UpdateTrack(TrackApiModel trackApiModel);
    Task<bool> DeleteTrack(int id);
    Task<PagedList<TrackApiModel>> GetTrackByArtistId(int id, int pageNumber, int pageSize);
    Task<PagedList<TrackApiModel>> GetTrackByInvoiceId(int id, int pageNumber, int pageSize);
}
```

## UPDATE CHINOOKSUPERVISOR PARTIAL CLASS FILES IN DOMAIN PROJECT

```csharp
public partial class ChinookSupervisor
{
    public async Task<PagedList<AlbumApiModel>> GetAllAlbum(int pageNumber, int pageSize) // todo
    {
        var albums = await _albumRepository.GetAll(pageNumber, pageSize);
        var albumApiModels = albums.ConvertAll<AlbumApiModel>();

        foreach (var album in albumApiModels)
        {
            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("Album-", album.Id), album, (TimeSpan)cacheEntryOptions);
        }

        var newPagedList = new PagedList<AlbumApiModel>(albumApiModels.ToList(), albums.TotalCount, albums.CurrentPage, albums.PageSize);
        return newPagedList;
    }

    public async Task<AlbumApiModel?> GetAlbumById(int id)
    {
        var albumApiModelCached = _cache.Get<AlbumApiModel>(string.Concat("Album-", id));

        if (albumApiModelCached != null)
        {
            return albumApiModelCached;
        }
        else
        {
            var album = await _albumRepository.GetById(id);
            if (album == null) return null;
            var albumApiModel = album.Convert();
            var result = (_artistRepository.GetById(album.ArtistId)).Result;
            if (result != null)
                albumApiModel.ArtistName = result.Name;
            //albumApiModel.Tracks = (await GetTrackByAlbumId(id) ?? Array.Empty<TrackApiModel>()).ToList();

            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("Album-", albumApiModel.Id), albumApiModel, (TimeSpan)cacheEntryOptions);

            return albumApiModel;
        }
    }

    public async Task<PagedList<AlbumApiModel>> GetAlbumByArtistId(int id, int pageNumber, int pageSize)
    {
        var albums = await _albumRepository.GetByArtistId(id, pageNumber, pageSize);
        var albumApiModels = albums.ConvertAll<AlbumApiModel>();
        var newPagedList = new PagedList<AlbumApiModel>(albumApiModels.ToList(), albums.TotalCount, albums.CurrentPage, albums.PageSize);
        return newPagedList;
    }

    public async Task<AlbumApiModel> AddAlbum(AlbumApiModel newAlbumApiModel)
    {
        await _albumValidator.ValidateAndThrowAsync(newAlbumApiModel);

        var album = newAlbumApiModel.Convert();

        album = await _albumRepository.Add(album);
        newAlbumApiModel.Id = album.Id;
        return newAlbumApiModel;
    }

    public async Task<bool> UpdateAlbum(AlbumApiModel albumApiModel)
    {
        await _albumValidator.ValidateAndThrowAsync(albumApiModel);

        var album = await _albumRepository.GetById(albumApiModel.Id);

        if (album is null) return false;
        album.Id = albumApiModel.Id;
        album.Title = albumApiModel.Title;
        album.ArtistId = albumApiModel.ArtistId;

        return await _albumRepository.Update(album);
    }

    public Task<bool> DeleteAlbum(int id)
        => _albumRepository.Delete(id);
}
```

## UPDATE CONTROLLERS IN API PROJECT

```csharp
//[Authorize]
[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[ApiVersion("1.0")]
public class AlbumController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<AlbumController> _logger;

    public AlbumController(IChinookSupervisor chinookSupervisor, ILogger<AlbumController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces(typeof(List<AlbumApiModel>))]
    public async Task<ActionResult<PagedList<AlbumApiModel>>> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var albums = await _chinookSupervisor.GetAllAlbum(pageNumber, pageSize);

            if (albums.Any())
            {
                var metadata = new
                {
                    albums.TotalCount,
                    albums.PageSize,
                    albums.CurrentPage,
                    albums.TotalPages,
                    albums.HasNext,
                    albums.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                
                
                
                return Ok(albums);
            }

            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"{problemDetails.Detail}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 404,
            };
        }
        catch (AlbumProblemException ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"{problemDetails.Detail}: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
        catch (Exception ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Get All Album action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
    }

    [HttpGet("{id}", Name = "GetAlbumById")]
    public async Task<ActionResult<AlbumApiModel>> Get(int id)
    {
        try
        {
            var album = await _chinookSupervisor.GetAlbumById(id);

            if (album != null)
            {
                return Ok(album);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "Album Not Found");
        }
        catch (AlbumProblemException ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"{problemDetails.Detail}: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AlbumApiModel>> Post([FromBody] AlbumApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Album is null");
            }

            return Ok(await _chinookSupervisor.AddAlbum(input));
        }
        catch (ValidationException ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Add Album action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
        catch (AlbumProblemException ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"{problemDetails.Detail}: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
        catch (Exception ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Add Album action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AlbumApiModel>> Put(int id, [FromBody] AlbumApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Album is null");
            }

            return Ok(await _chinookSupervisor.UpdateAlbum(input));
        }
        catch (ValidationException ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Update Album action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
        catch (Exception ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Update Album action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeleteAlbum(id));
        }
        catch (Exception ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Delete action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
    }

    [HttpGet("artist/{id}")]
    public async Task<ActionResult<PagedList<AlbumApiModel>>> GetByArtistId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var albums = await _chinookSupervisor.GetAlbumByArtistId(id, pageNumber, pageSize);

            if (albums.Any())
            {
                var metadata = new
                {
                    albums.TotalCount,
                    albums.PageSize,
                    albums.CurrentPage,
                    albums.TotalPages,
                    albums.HasNext,
                    albums.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                
                return Ok(albums);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Albums Could Be Found for the Artist");
        }
        catch (Exception ex)
        {
            var problemDetails = new AlbumProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://example.com/api/Artist/not-found",
                Title = "Could not find any artists",
                Detail = "Something went wrong inside the ArtistController Get All action",
                AlbumId = null,
                Instance = HttpContext.Request.Path
            };
            _logger.LogError($"Something went wrong inside the AlbumController Get By Artist action: {ex}");
            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 403,
            };
        }
    }
}
```
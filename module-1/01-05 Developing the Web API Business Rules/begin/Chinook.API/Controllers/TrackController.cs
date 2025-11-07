using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrackController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<TrackController> _logger;

    public TrackController(IChinookSupervisor chinookSupervisor, ILogger<TrackController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<TrackApiModel>>> Get()
    {
        var tracks = await _chinookSupervisor.GetAllTrack();

        if (tracks.Any())
        {
            return Ok(tracks);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetTrackById")]
    [Produces("application/json")]
    public async Task<ActionResult<TrackApiModel>> Get(int id)
    {
        var track = await _chinookSupervisor.GetTrackById(id);

        if (track != null)
        {
            return Ok(track);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Track Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<TrackApiModel>> Post([FromBody] TrackApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Track is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddTrack(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<TrackApiModel>> Put(int id, [FromBody] TrackApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Track is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateTrack(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteTrack(id));
    }

    [HttpGet("artist/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<TrackApiModel>>> GetByArtistId(int id)
    {
        var tracks = await _chinookSupervisor.GetTrackByArtistId(id);

        if (tracks.Any())
        {
            return Ok(tracks);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Artist");
        }
    }

    [HttpGet("invoice/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<TrackApiModel>>> GetByInvoiceId(int id)
    {
        var tracks = await _chinookSupervisor.GetTrackByInvoiceId(id);

        if (tracks.Any())
        {
            return Ok(tracks);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Invoice");
        }
    }

    [HttpGet("album/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<TrackApiModel>>> GetByAlbumId(int id)
    {
        var tracks = await _chinookSupervisor.GetTrackByAlbumId(id);

        if (tracks.Any())
        {
            return Ok(tracks);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Album");
        }
    }

    [HttpGet("mediatype/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<TrackApiModel>>> GetByMediaTypeId(int id)
    {
        var tracks = await _chinookSupervisor.GetTrackByMediaTypeId(id);

        if (tracks.Any())
        {
            return Ok(tracks);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Media Type");
        }
    }

    [HttpGet("genre/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<TrackApiModel>>> GetByGenreId(int id)
    {
        var tracks = await _chinookSupervisor.GetTrackByGenreId(id);

        if (tracks.Any())
        {
            return Ok(tracks);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Genre");
        }
    }
}
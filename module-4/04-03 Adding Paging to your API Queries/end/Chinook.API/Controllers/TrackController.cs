using System.Net;
using System.Text.Json;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Extensions;
using Chinook.Domain.Supervisor;
using FluentValidation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[ApiVersion("1.0")]
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
    public async Task<ActionResult<PagedList<TrackApiModel>>> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var tracks = await _chinookSupervisor.GetAllTrack(pageNumber, pageSize);

            if (tracks.Any())
            {
                var metadata = new
                {
                    tracks.TotalCount,
                    tracks.PageSize,
                    tracks.CurrentPage,
                    tracks.TotalPages,
                    tracks.HasNext,
                    tracks.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(tracks);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Get action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Tracks");
        }
    }

    [HttpGet("{id}", Name = "GetTrackById")]
    [Produces("application/json")]
    public async Task<ActionResult<TrackApiModel>> Get(int id)
    {
        try
        {
            var track = await _chinookSupervisor.GetTrackById(id);

            if (track != null)
            {
                return Ok(track);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "Track Not Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController GetById action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get Track By Id");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<TrackApiModel>> Post([FromBody] TrackApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Track is null");
            }

            return Ok(await _chinookSupervisor.AddTrack(input));
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Add Track action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError, "Error occurred while executing Add Tracks");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Add Track action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError, "Error occurred while executing Add Tracks");
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<TrackApiModel>> Put(int id, [FromBody] TrackApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Track is null");
            }

            return Ok(await _chinookSupervisor.UpdateTrack(input));
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Update Track action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update Tracks");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Update Track action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update Tracks");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeleteTrack(id));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Delete action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Delete Track");
        }
    }

    [HttpGet("artist/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<TrackApiModel>>> GetByArtistId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var tracks = await _chinookSupervisor.GetTrackByArtistId(id, pageNumber, pageSize);

            if (tracks.Any())
            {
                var metadata = new
                {
                    tracks.TotalCount,
                    tracks.PageSize,
                    tracks.CurrentPage,
                    tracks.TotalPages,
                    tracks.HasNext,
                    tracks.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(tracks);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Artist");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Get By Artist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Tracks for Artist");
        }
    }

    [HttpGet("invoice/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<TrackApiModel>>> GetByInvoiceId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var tracks = await _chinookSupervisor.GetTrackByInvoiceId(id, pageNumber, pageSize);

            if (tracks.Any())
            {
                var metadata = new
                {
                    tracks.TotalCount,
                    tracks.PageSize,
                    tracks.CurrentPage,
                    tracks.TotalPages,
                    tracks.HasNext,
                    tracks.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(tracks);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Invoice");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Get By Invoice action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Tracks for Invoice");
        }
    }

    [HttpGet("album/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<TrackApiModel>>> GetByAlbumId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var tracks = await _chinookSupervisor.GetTrackByAlbumId(id, pageNumber, pageSize);

            if (tracks.Any())
            {
                var metadata = new
                {
                    tracks.TotalCount,
                    tracks.PageSize,
                    tracks.CurrentPage,
                    tracks.TotalPages,
                    tracks.HasNext,
                    tracks.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(tracks);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Album");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Get By Album action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Tracks for Album");
        }
    }

    [HttpGet("mediatype/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<TrackApiModel>>> GetByMediaTypeId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var tracks = await _chinookSupervisor.GetTrackByMediaTypeId(id, pageNumber, pageSize);

            if (tracks.Any())
            {
                var metadata = new
                {
                    tracks.TotalCount,
                    tracks.PageSize,
                    tracks.CurrentPage,
                    tracks.TotalPages,
                    tracks.HasNext,
                    tracks.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(tracks);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Media Type");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Get By Media Type action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Tracks for Media Type");
        }
    }

    [HttpGet("genre/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<TrackApiModel>>> GetByGenreId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var tracks = await _chinookSupervisor.GetTrackByGenreId(id, pageNumber, pageSize);

            if (tracks.Any())
            {
                var metadata = new
                {
                    tracks.TotalCount,
                    tracks.PageSize,
                    tracks.CurrentPage,
                    tracks.TotalPages,
                    tracks.HasNext,
                    tracks.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(tracks);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Tracks Could Be Found for the Genre");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the TrackController Get By Genre action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Tracks for Genre");
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<TrackApiModel>> Patch(int id, [FromBody] JsonPatchDocument<TrackApiModel> input)
    {
        var track = await _chinookSupervisor.GetTrackById(id);

        if (track == null)
        {
            return NotFound();
        }

        input.ApplyTo(track, ModelState); // Must have Microsoft.AspNetCore.Mvc.NewtonsoftJson installed  

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _chinookSupervisor.UpdateTrack(track); //Update in the database
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }

        return Ok(track);
    }
}
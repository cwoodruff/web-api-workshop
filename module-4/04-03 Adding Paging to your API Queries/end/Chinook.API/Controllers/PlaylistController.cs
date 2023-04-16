using System.Net;
using System.Text.Json;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Extensions;
using Chinook.Domain.Supervisor;
using FluentValidation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[ApiVersion("1.0")]
public class PlaylistController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<PlaylistController> _logger;

    public PlaylistController(IChinookSupervisor chinookSupervisor, ILogger<PlaylistController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<PlaylistApiModel>>> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var playlists = await _chinookSupervisor.GetAllPlaylist(pageNumber, pageSize);

            if (playlists.Any())
            {
                var metadata = new
                {
                    playlists.TotalCount,
                    playlists.PageSize,
                    playlists.CurrentPage,
                    playlists.TotalPages,
                    playlists.HasNext,
                    playlists.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(playlists);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "No Playlists Could Be Found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController Get action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Playlists");
        }
    }

    [HttpGet("{id}", Name = "GetPlaylistById")]
    [Produces("application/json")]
    public async Task<ActionResult<PlaylistApiModel>> Get(int id)
    {
        try
        {
            var playlist = await _chinookSupervisor.GetPlaylistById(id);

            if (playlist != null)
            {
                return Ok(playlist);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "Playlist Not Found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController GetById action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get Playlist By Id");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<PlaylistApiModel>> Post([FromBody] PlaylistApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Playlist is null");
            }
            else
            {
                return Ok(await _chinookSupervisor.AddPlaylist(input));
            }
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController Add Playlist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Playlists");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController Add Playlist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Playlists");
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<PlaylistApiModel>> Put(int id, [FromBody] PlaylistApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Playlist is null");
            }
            else
            {
                return Ok(await _chinookSupervisor.UpdatePlaylist(input));
            }
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController Add Playlist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Playlists");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController Add Playlist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Playlists");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeletePlaylist(id));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the PlaylistController GetById action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get Playlist By Id");
        }
    }
}
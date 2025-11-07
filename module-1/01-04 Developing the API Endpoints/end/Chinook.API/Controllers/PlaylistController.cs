using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    public async Task<ActionResult<List<PlaylistApiModel>>> Get()
    {
        var playlists = await _chinookSupervisor.GetAllPlaylist();

        if (playlists.Any())
        {
            return Ok(playlists);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Playlists Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetPlaylistById")]
    [Produces("application/json")]
    public async Task<ActionResult<PlaylistApiModel>> Get(int id)
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

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<PlaylistApiModel>> Post([FromBody] PlaylistApiModel input)
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

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<PlaylistApiModel>> Put(int id, [FromBody] PlaylistApiModel input)
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

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeletePlaylist(id));
    }
}
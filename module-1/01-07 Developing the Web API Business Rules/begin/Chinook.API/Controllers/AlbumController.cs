using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
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
    public async Task<ActionResult<List<AlbumApiModel>>> Get()
    {
        var albums = await _chinookSupervisor.GetAllAlbum();

        if (albums.Any())
        {
            return Ok(albums);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Albums Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetAlbumById")]
    public async Task<ActionResult<AlbumApiModel>> Get(int id)
    {
        var album = await _chinookSupervisor.GetAlbumById(id);

        if (album != null)
        {
            return Ok(album);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Album Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AlbumApiModel>> Post([FromBody] AlbumApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Album is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddAlbum(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AlbumApiModel>> Put(int id, [FromBody] AlbumApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Album is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateAlbum(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteAlbum(id));
    }

    [HttpGet("artist/{id}")]
    public async Task<ActionResult<List<AlbumApiModel>>> GetByArtistId(int id)
    {
        var albums = await _chinookSupervisor.GetAlbumByArtistId(id);

        if (albums.Any())
        {
            return Ok(albums);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Albums Could Be Found for the Artist");
        }
    }
}
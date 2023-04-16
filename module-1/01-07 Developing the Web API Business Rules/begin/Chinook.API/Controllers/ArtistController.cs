using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArtistController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<ArtistController> _logger;

    public ArtistController(IChinookSupervisor chinookSupervisor, ILogger<ArtistController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<ArtistApiModel>>> Get()
    {
        var artists = await _chinookSupervisor.GetAllArtist();

        if (artists.Any())
        {
            return Ok(artists);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Artists Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetArtistById")]
    [Produces("application/json")]
    public async Task<ActionResult<ArtistApiModel>> Get(int id)
    {
        var artist = await _chinookSupervisor.GetArtistById(id);

        if (artist != null)
        {
            return Ok(artist);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Artist Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<ArtistApiModel>> Post([FromBody] ArtistApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Artist is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddArtist(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<ArtistApiModel>> Put(int id, [FromBody] ArtistApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Artist is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateArtist(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteArtist(id));
    }
}
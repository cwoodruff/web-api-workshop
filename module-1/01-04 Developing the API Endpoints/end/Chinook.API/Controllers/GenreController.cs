using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GenreController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<GenreController> _logger;

    public GenreController(IChinookSupervisor chinookSupervisor, ILogger<GenreController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<GenreApiModel>>> Get()
    {
        var genres = await _chinookSupervisor.GetAllGenre();

        if (genres.Any())
        {
            return Ok(genres);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Genres Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetGenreById")]
    [Produces("application/json")]
    public async Task<ActionResult<GenreApiModel>> Get(int id)
    {
        var genre = await _chinookSupervisor.GetGenreById(id);

        if (genre != null)
        {
            return Ok(genre);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Genre Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<GenreApiModel>> Post([FromBody] GenreApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Genre is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddGenre(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<GenreApiModel>> Put(int id, [FromBody] GenreApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Genre is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateGenre(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteGenre(id));
    }
}
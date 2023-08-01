using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaTypeController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<MediaTypeController> _logger;

    public MediaTypeController(IChinookSupervisor chinookSupervisor, ILogger<MediaTypeController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<MediaTypeApiModel>>> Get()
    {
        var mediaTypes = await _chinookSupervisor.GetAllMediaType();

        if (mediaTypes.Any())
        {
            return Ok(mediaTypes);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No MediaType Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetMediaTypeById")]
    [Produces("application/json")]
    public async Task<ActionResult<MediaTypeApiModel>> Get(int id)
    {
        var mediaType = await _chinookSupervisor.GetMediaTypeById(id);

        if (mediaType != null)
        {
            return Ok(mediaType);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "MediaType Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<MediaTypeApiModel>> Post([FromBody] MediaTypeApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given MediaType is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddMediaType(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<MediaTypeApiModel>> Put(int id, [FromBody] MediaTypeApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given MediaType is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateMediaType(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteMediaType(id));
    }
}
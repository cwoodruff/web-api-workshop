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
[ResponseCache(Duration = 604800)]
[ApiVersion("1.0")]
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
    public async Task<ActionResult<PagedList<MediaTypeApiModel>>> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var mediaTypes = await _chinookSupervisor.GetAllMediaType(pageNumber, pageSize);

            if (mediaTypes.Any())
            {
                var metadata = new
                {
                    mediaTypes.TotalCount,
                    mediaTypes.PageSize,
                    mediaTypes.CurrentPage,
                    mediaTypes.TotalPages,
                    mediaTypes.HasNext,
                    mediaTypes.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(mediaTypes);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No MediaType Could Be Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController Get action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All MediaType");
        }
    }

    [HttpGet("{id}", Name = "GetMediaTypeById")]
    [Produces("application/json")]
    public async Task<ActionResult<MediaTypeApiModel>> Get(int id)
    {
        try
        {
            var mediaType = await _chinookSupervisor.GetMediaTypeById(id);

            if (mediaType != null)
            {
                return Ok(mediaType);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "MediaType Not Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController GetById action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get MediaType By Id");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<MediaTypeApiModel>> Post([FromBody] MediaTypeApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given MediaType is null");
            }

            return Ok(await _chinookSupervisor.AddMediaType(input));
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController Add MediaType action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add MediaType");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController Add MediaType action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add MediaType");
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<MediaTypeApiModel>> Put(int id, [FromBody] MediaTypeApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given MediaType is null");
            }

            return Ok(await _chinookSupervisor.UpdateMediaType(input));
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController Update MediaType action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update MediaType");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController Update MediaType action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update MediaType");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeleteMediaType(id));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the MediaTypeController Delete action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Delete MediaType");
        }
    }
}
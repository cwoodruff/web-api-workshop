﻿using System.Net;
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
    public async Task<ActionResult<PagedList<ArtistApiModel>>> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var artists = await _chinookSupervisor.GetAllArtist(pageNumber, pageSize);

            if (artists.Any())
            {
                return Ok(artists);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound, "No Artists Could Be Found");
            }
        }
        catch (Exception ex)  
        {  
            _logger.LogError($"Something went wrong inside the ArtistController Get action: {ex}");  
            return StatusCode((int)HttpStatusCode.InternalServerError, "Error occurred while executing Get All Artists");  
        }  
    }

    [HttpGet("{id}", Name = "GetArtistById")]
    [Produces("application/json")]
    public async Task<ActionResult<ArtistApiModel>> Get(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the ArtistController GetById action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get Artist By Id");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<ArtistApiModel>> Post([FromBody] ArtistApiModel input)
    {
        try
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
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the ArtistController Add Artist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Artists");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the ArtistController Add Artist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Artists");
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<ArtistApiModel>> Put(int id, [FromBody] ArtistApiModel input)
    {
        try
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
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the ArtistController Update Artist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update Artists");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the ArtistController Update Artist action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update Artists");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeleteArtist(id));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the ArtistController Delete action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Delete Artist");
        }
    }
}
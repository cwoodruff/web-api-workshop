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
public class InvoiceController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(IChinookSupervisor chinookSupervisor, ILogger<InvoiceController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<InvoiceApiModel>>> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var invoices = await _chinookSupervisor.GetAllInvoice(pageNumber, pageSize);

            if (invoices.Any())
            {
                var metadata = new
                {
                    invoices.TotalCount,
                    invoices.PageSize,
                    invoices.CurrentPage,
                    invoices.TotalPages,
                    invoices.HasNext,
                    invoices.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(invoices);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Invoices Could Be Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController Get action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Invoices");
        }
    }

    [HttpGet("{id}", Name = "GetInvoiceById")]
    [Produces("application/json")]
    public async Task<ActionResult<InvoiceApiModel>> Get(int id)
    {
        try
        {
            var invoice = await _chinookSupervisor.GetInvoiceById(id);

            if (invoice != null)
            {
                return Ok(invoice);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "Invoice Not Found");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController GetById action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get Invoice By Id");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<InvoiceApiModel>> Post([FromBody] InvoiceApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Invoice is null");
            }

            return Ok(await _chinookSupervisor.AddInvoice(input));
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController Add Invoice action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Invoices");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController Add Invoice action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Add Invoices");
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<InvoiceApiModel>> Put(int id, [FromBody] InvoiceApiModel input)
    {
        try
        {
            if (input == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Given Invoice is null");
            }

            return Ok(await _chinookSupervisor.UpdateInvoice(input));
        }
        catch (ValidationException ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController Update Invoice action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update Invoices");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController Update Invoice action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Update Invoices");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeleteInvoice(id));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController Delete action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Delete Invoice");
        }
    }

    [HttpGet("employee/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<InvoiceApiModel>>> GetByEmployeeId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var invoices = await _chinookSupervisor.GetInvoiceByEmployeeId(id, pageNumber, pageSize);

            if (invoices.Any())
            {
                var metadata = new
                {
                    invoices.TotalCount,
                    invoices.PageSize,
                    invoices.CurrentPage,
                    invoices.TotalPages,
                    invoices.HasNext,
                    invoices.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(invoices);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Invoices Could Be Found for the Employee");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController GetByEmployeeId action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Invoices for Employee");
        }
    }

    [HttpGet("customer/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<PagedList<InvoiceApiModel>>> GetByCustomerId(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var invoices = await _chinookSupervisor.GetInvoiceByCustomerId(id, pageNumber, pageSize);

            if (invoices.Any())
            {
                var metadata = new
                {
                    invoices.TotalCount,
                    invoices.PageSize,
                    invoices.CurrentPage,
                    invoices.TotalPages,
                    invoices.HasNext,
                    invoices.HasPrevious
                };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));
                return Ok(invoices);
            }

            return StatusCode((int)HttpStatusCode.NotFound, "No Invoices Could Be Found for the Customer");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the InvoiceController GetByCustomerId action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Get All Invoices for Customer");
        }
    }
}
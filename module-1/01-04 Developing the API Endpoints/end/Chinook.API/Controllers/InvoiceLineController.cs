using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceLineController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<InvoiceLineController> _logger;

    public InvoiceLineController(IChinookSupervisor chinookSupervisor, ILogger<InvoiceLineController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<InvoiceLineApiModel>>> Get()
    {
        var invoiceLines = await _chinookSupervisor.GetAllInvoiceLine();

        if (invoiceLines.Any())
        {
            return Ok(invoiceLines);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No InvoiceLines Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetInvoiceLineById")]
    [Produces("application/json")]
    public async Task<ActionResult<InvoiceLineApiModel>> Get(int id)
    {
        var invoiceLine = await _chinookSupervisor.GetInvoiceLineById(id);

        if (invoiceLine != null)
        {
            return Ok(invoiceLine);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "InvoiceLine Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<InvoiceLineApiModel>> Post([FromBody] InvoiceLineApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given InvoiceLine is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddInvoiceLine(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<InvoiceLineApiModel>> Put(int id, [FromBody] InvoiceLineApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given InvoiceLine is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateInvoiceLine(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteInvoiceLine(id));
    }

    [HttpGet("invoice/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<InvoiceLineApiModel>>> GetByInvoiceId(int id)
    {
        var invoiceLines = await _chinookSupervisor.GetInvoiceLineByInvoiceId(id);

        if (invoiceLines.Any())
        {
            return Ok(invoiceLines);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No InvoiceLines Could Be Found for the Invoice");
        }
    }

    [HttpGet("track/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<InvoiceLineApiModel>>> GetByTrackId(int id)
    {
        var invoiceLines = await _chinookSupervisor.GetInvoiceLineByTrackId(id);

        if (invoiceLines.Any())
        {
            return Ok(invoiceLines);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No InvoiceLines Could Be Found for the Track");
        }
    }
}
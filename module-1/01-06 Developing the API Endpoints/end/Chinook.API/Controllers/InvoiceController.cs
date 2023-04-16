using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    public async Task<ActionResult<List<InvoiceApiModel>>> Get()
    {
        var invoices = await _chinookSupervisor.GetAllInvoice();

        if (invoices.Any())
        {
            return Ok(invoices);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Invoices Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetInvoiceById")]
    [Produces("application/json")]
    public async Task<ActionResult<InvoiceApiModel>> Get(int id)
    {
        var invoice = await _chinookSupervisor.GetInvoiceById(id);

        if (invoice != null)
        {
            return Ok(invoice);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Invoice Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<InvoiceApiModel>> Post([FromBody] InvoiceApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Invoice is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddInvoice(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<InvoiceApiModel>> Put(int id, [FromBody] InvoiceApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Invoice is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateInvoice(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteInvoice(id));
    }

    [HttpGet("employee/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<InvoiceApiModel>>> GetByEmployeeId(int id)
    {
        var invoices = await _chinookSupervisor.GetInvoiceByEmployeeId(id);

        if (invoices.Any())
        {
            return Ok(invoices);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Invoices Could Be Found for the Employee");
        }
    }

    [HttpGet("customer/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<InvoiceApiModel>>> GetByCustomerId(int id)
    {
        var invoices = await _chinookSupervisor.GetInvoiceByCustomerId(id);

        if (invoices.Any())
        {
            return Ok(invoices);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Invoices Could Be Found for the Customer");
        }
    }
}
using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(IChinookSupervisor chinookSupervisor, ILogger<CustomerController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<CustomerApiModel>>> Get()
    {
        var customers = await _chinookSupervisor.GetAllCustomer();

        if (customers.Any())
        {
            return Ok(customers);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Customers Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetCustomerById")]
    [Produces("application/json")]
    public async Task<ActionResult<CustomerApiModel>> Get(int id)
    {
        var customer = await _chinookSupervisor.GetCustomerById(id);

        if (customer != null)
        {
            return Ok(customer);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Customer Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<CustomerApiModel>> Post([FromBody] CustomerApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Customer is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddCustomer(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<CustomerApiModel>> Put(int id, [FromBody] CustomerApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Customer is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateCustomer(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        return Ok(await _chinookSupervisor.DeleteCustomer(id));
    }

    [HttpGet("supportrep/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<EmployeeApiModel>> GetBySupportRepId(int id)
    {
        var employee = await _chinookSupervisor.GetEmployeeById(id);

        if (employee != null)
        {
            return Ok(employee);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Support Rep Could Be Found");
        }
    }
}
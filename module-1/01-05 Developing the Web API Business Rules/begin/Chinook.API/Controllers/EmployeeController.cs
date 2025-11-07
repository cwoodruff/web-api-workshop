using System.Net;
using Chinook.Domain.ApiModels;
using Chinook.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly IChinookSupervisor _chinookSupervisor;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IChinookSupervisor chinookSupervisor, ILogger<EmployeeController> logger)
    {
        _chinookSupervisor = chinookSupervisor;
        _logger = logger;
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<List<EmployeeApiModel>>> Get()
    {
        var employees = await _chinookSupervisor.GetAllEmployee();

        if (employees.Any())
        {
            return Ok(employees);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Employees Could Be Found");
        }
    }

    [HttpGet("{id}", Name = "GetEmployeeById")]
    [Produces("application/json")]
    public async Task<ActionResult<EmployeeApiModel>> Get(int id)
    {
        var employee = await _chinookSupervisor.GetEmployeeById(id);

        if (employee != null)
        {
            return Ok(employee);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "Employee Not Found");
        }
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<EmployeeApiModel>> Post([FromBody] EmployeeApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Employee is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.AddEmployee(input));
        }
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<EmployeeApiModel>> Put(int id, [FromBody] EmployeeApiModel input)
    {
        if (input == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "Given Employee is null");
        }
        else
        {
            return Ok(await _chinookSupervisor.UpdateEmployee(input));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            return Ok(await _chinookSupervisor.DeleteEmployee(id));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside the EmployeeController Delete action: {ex}");
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "Error occurred while executing Delete Employee");
        }
    }

    [HttpGet("reportsto/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<EmployeeApiModel>> GetReportsTo(int id)
    {
        var employee = await _chinookSupervisor.GetEmployeeById(id);

        if (employee != null)
        {
            return Ok(employee);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound,
                "No Reporting Employees Could Be Found for the Employee");
        }
    }

    [HttpGet("directreports/{id}")]
    [Produces("application/json")]
    public async Task<ActionResult<List<EmployeeApiModel>>> GetDirectReports(int id)
    {
        var employees = await _chinookSupervisor.GetDirectReports(id);

        if (employees.Any())
        {
            return Ok(employees);
        }
        else
        {
            return StatusCode((int)HttpStatusCode.NotFound, "No Employees Could Be Found");
        }
    }
}
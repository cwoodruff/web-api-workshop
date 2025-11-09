using Microsoft.AspNetCore.Mvc;

namespace Chinook.API.Controllers;

[ApiVersionNeutral]
[Route("")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }
}
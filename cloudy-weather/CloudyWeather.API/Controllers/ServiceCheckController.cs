using Microsoft.AspNetCore.Mvc;

namespace CloudyWeather.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceCheckController(IConfiguration configuration) : ControllerBase
{
    private readonly string _environment = configuration.GetValue<string>("SYS_ENVIRONMENT") ?? "";

    [HttpGet("")]
    public IActionResult GetEnvironment()
        => Ok($"The current environment is: {_environment}");
}

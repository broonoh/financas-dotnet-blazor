using Microsoft.AspNetCore.Mvc;

namespace MinhasFinancas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200)]
    public IActionResult Get()
        => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}

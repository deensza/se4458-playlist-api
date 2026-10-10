using Microsoft.AspNetCore.Mvc;

namespace PlaylistApi.Controllers;

/// <summary>
/// Simple "hello world" endpoint to check that the API is up and running.
/// </summary>
[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        service = "Playlist API",
        timeUtc = DateTime.UtcNow
    });
}

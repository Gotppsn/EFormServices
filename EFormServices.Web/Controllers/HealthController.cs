// EFormServices.Web/Controllers/HealthController.cs
// Got code 30/05/2025
using EFormServices.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                database = "Connected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                database = "Disconnected",
                error = ex.Message
            });
        }
    }
}
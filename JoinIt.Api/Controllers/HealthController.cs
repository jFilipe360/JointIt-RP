using JoinIt.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Api.Controllers;

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
        var databaseConnected = await _context.Database.CanConnectAsync();

        if (!databaseConnected)
        {
            return StatusCode(503, new
            {
                status = "error",
                database = "disconnected"
            });
        }

        var categorias = await _context.Categorias.CountAsync();

        return Ok(new
        {
            status = "ok",
            database = "connected",
            categorias
        });
    }
}
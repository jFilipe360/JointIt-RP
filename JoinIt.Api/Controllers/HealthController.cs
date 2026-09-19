using JoinIt.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Api.Controllers;

// Endpoint de diagnóstico usado para verificar o estado da API e da base de dados
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Verifica se a aplicação consegue estabelecer ligação à base de dados
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

        // Executa uma consulta simples para confirmar o acesso aos dados
        var categorias = await _context.Categorias.CountAsync();

        return Ok(new
        {
            status = "ok",
            database = "connected",
            categorias
        });
    }
}
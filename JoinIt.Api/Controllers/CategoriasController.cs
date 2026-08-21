using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Categorias;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nome = c.Nome
            })
            .ToListAsync();

        return Ok(categorias);
    }
}
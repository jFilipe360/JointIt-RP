using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Categorias;
using JoinIt.Api.DTOs.Eventos;
using JoinIt.Api.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JoinIt.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResumoDto>>> GetUsers()
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var utilizadores = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != utilizadorId)
            .OrderBy(u => u.Nome)
            .Select(u => new UserResumoDto
            {
                Id = u.Id,
                Nome = u.Nome,
                FotoPerfil = u.FotoPerfil
            })
            .ToListAsync();

        return Ok(utilizadores);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetalhesDto>> GetUser(string id)
    {
        var utilizadorAtualId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorAtualId == null)
        {
            return Unauthorized();
        }

        var utilizador = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDetalhesDto
            {
                Id = u.Id,
                Nome = u.Nome,
                FotoPerfil = u.FotoPerfil,

                EventosPublicos = u.EventosCriados
                    .Where(e => !e.IsPrivado)
                    .OrderBy(e => e.DataHora)
                    .Select(e => new EventoResumoDto
                    {
                        Id = e.Id,
                        Titulo = e.Titulo,
                        Descricao = e.Descricao,
                        DataHora = e.DataHora,
                        DataFim = e.DataFim,
                        Local = e.Local,
                        Morada = e.Morada,
                        IsPrivado = e.IsPrivado,
                        NumMaxParticipantes = e.NumMaxParticipantes,
                        NumParticipantes = e.Participantes.Count(),
                        VagasDisponiveis =
                            e.NumMaxParticipantes - e.Participantes.Count(),
                        Estado = e.Estado,
                        CriadorNome = u.Nome,
                        Categorias = e.EventosCategorias
                            .Select(ec => new CategoriaDto
                            {
                                Id = ec.Categoria.Id,
                                Nome = ec.Categoria.Nome
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (utilizador == null)
        {
            return NotFound(new
            {
                message = "Utilizador não encontrado."
            });
        }

        return Ok(utilizador);
    }
}
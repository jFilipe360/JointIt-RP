using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Categorias;
using JoinIt.Api.DTOs.Eventos;
using JoinIt.Api.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Api.Controllers;

[ApiController]
[Route("api/eventos")]
public class EventosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginadoDto<EventoResumoDto>>> GetEventos(
        string? pesquisa = null,
        int? categoriaId = null,
        string? estado = null,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        if (pageSize > 50)
        {
            pageSize = 50;
        }

        var query = _context.Eventos
            .AsNoTracking()
            .Where(e => !e.IsPrivado);

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            pesquisa = pesquisa.Trim();

            query = query.Where(e =>
                e.Titulo.Contains(pesquisa) ||
                e.Descricao.Contains(pesquisa) ||
                e.Local.Contains(pesquisa) ||
                (e.Morada != null && e.Morada.Contains(pesquisa)));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(e =>
                e.EventosCategorias.Any(ec =>
                    ec.Categoria.Id == categoriaId.Value));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!Enum.TryParse<EstadoEvento>(estado, true, out var estadoEvento))
            {
                return BadRequest(new
                {
                    message = "Estado inválido."
                });
            }

            query = query.Where(e => e.Estado == estadoEvento);
        }

        var totalItems = await query.CountAsync();

        var eventos = await query
            .OrderBy(e => e.DataHora)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                CriadorNome = e.Criador.Nome ?? string.Empty,
                Categorias = e.EventosCategorias
                    .Select(ec => new CategoriaDto
                    {
                        Id = ec.Categoria.Id,
                        Nome = ec.Categoria.Nome
                    })
                    .ToList()
            })
            .ToListAsync();



        return Ok(new ResultadoPaginadoDto<EventoResumoDto>
        {
            Items = eventos,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventoDetalhesDto>> GetEvento(int id)
    {
        var evento = await _context.Eventos
            .AsNoTracking()
            .Where(e => e.Id == id && !e.IsPrivado)
            .Select(e => new EventoDetalhesDto
            {
                Id = e.Id,
                Titulo = e.Titulo,
                Descricao = e.Descricao,
                DataHora = e.DataHora,
                DataFim = e.DataFim,
                Local = e.Local,
                Morada = e.Morada,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                IsPrivado = e.IsPrivado,
                NumMaxParticipantes = e.NumMaxParticipantes,
                NumParticipantes = e.Participantes.Count(),
                VagasDisponiveis =
                    e.NumMaxParticipantes - e.Participantes.Count(),
                Estado = e.Estado,
                CriadorId = e.CriadorId,
                CriadorNome = e.Criador.Nome ?? string.Empty,
                Categorias = e.EventosCategorias
                    .Select(ec => new CategoriaDto
                    {
                        Id = ec.Categoria.Id,
                        Nome = ec.Categoria.Nome
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (evento == null)
        {
            return NotFound(new
            {
                message = "Evento não encontrado."
            });
        }

        return Ok(evento);
    }
}
using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Categorias;
using JoinIt.Api.DTOs.Eventos;
using JoinIt.Api.Enums;
using JoinIt.Api.Models;
using JoinIt.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JoinIt.Api.Controllers;

[ApiController]
[Route("api/eventos")]
public class EventosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEstadoEventoService _estadoEventoService;
    private readonly INotificacaoService _notificacaoService;

    public EventosController(
    ApplicationDbContext context,
    IEstadoEventoService estadoEventoService,
    INotificacaoService notificacaoService)
    {
        _context = context;
        _estadoEventoService = estadoEventoService;
        _notificacaoService = notificacaoService;
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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CriarEvento(CriarEventoDto dto)
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        if (dto.DataHora <= DateTime.Now)
        {
            return BadRequest(new
            {
                message = "A data de início deve ser futura."
            });
        }

        if (dto.CategoriaIds.Count == 0)
        {
            return BadRequest(new
            {
                message = "Seleciona pelo menos uma categoria."
            });
        }

        if (dto.DataFim <= dto.DataHora)
        {
            return BadRequest(new
            {
                message = "A data de fim deve ser posterior à data de início."
            });
        }

        var temLatitude = dto.Latitude.HasValue;
        var temLongitude = dto.Longitude.HasValue;

        if (temLatitude != temLongitude)
        {
            return BadRequest(new
            {
                message = "A latitude e a longitude devem ser preenchidas em conjunto."
            });
        }

        var categoriaIds = dto.CategoriaIds
            .Distinct()
            .ToList();

        if (categoriaIds.Count > 0)
        {
            var categoriasExistentes = await _context.Categorias
                .CountAsync(c => categoriaIds.Contains(c.Id));

            if (categoriasExistentes != categoriaIds.Count)
            {
                return BadRequest(new
                {
                    message = "Uma ou mais categorias não existem."
                });
            }
        }

        var evento = new Evento
        {
            Titulo = dto.Titulo.Trim(),
            Descricao = dto.Descricao.Trim(),
            DataHora = dto.DataHora,
            DataFim = dto.DataFim,
            Local = dto.Local.Trim(),
            Morada = string.IsNullOrWhiteSpace(dto.Morada)
                ? null
                : dto.Morada.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsPrivado = dto.IsPrivado,
            NumMaxParticipantes = dto.NumMaxParticipantes,
            Estado = EstadoEvento.ParaBreve,
            CriadorId = utilizadorId
        };

        foreach (var categoriaId in categoriaIds)
        {
            evento.EventosCategorias.Add(new EventoCategoria
            {
                CategoriaId = categoriaId
            });
        }

        evento.Participantes.Add(new Participante
        {
            UtilizadorId = utilizadorId,
            Estado = EstadoPedido.Aceite,
            DataPedido = DateTime.Now
        });

        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            id = evento.Id,
            message = "Evento criado com sucesso."
        });
    }

    [Authorize]
    [HttpGet("meus")]
    public async Task<ActionResult<IEnumerable<EventoResumoDto>>> GetMeusEventos()
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var eventos = await _context.Eventos
            .AsNoTracking()
            .Where(e => e.CriadorId == utilizadorId)
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

        return Ok(eventos);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditarEvento(
    int id,
    EditarEventoDto dto)
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        await _estadoEventoService.AtualizarEstadosAsync();

        var evento = await _context.Eventos
            .Include(e => e.EventosCategorias)
            .Include(e => e.Participantes)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null)
        {
            return NotFound(new
            {
                message = "Evento não encontrado."
            });
        }

        if (evento.CriadorId != utilizadorId)
        {
            return Forbid();
        }

        if (evento.Estado == EstadoEvento.Cancelado)
        {
            return BadRequest(new
            {
                message = "Não é possível editar um evento cancelado."
            });
        }

        if (evento.Estado == EstadoEvento.Terminado)
        {
            return BadRequest(new
            {
                message = "Não é possível editar um evento terminado."
            });
        }

        if (dto.DataHora <= DateTime.Now)
        {
            return BadRequest(new
            {
                message = "A data de início deve ser futura."
            });
        }

        if (dto.DataFim <= dto.DataHora)
        {
            return BadRequest(new
            {
                message = "A data de fim deve ser posterior à data de início."
            });
        }

        var temLatitude = dto.Latitude.HasValue;
        var temLongitude = dto.Longitude.HasValue;

        if (temLatitude != temLongitude)
        {
            return BadRequest(new
            {
                message = "Seleciona uma localização completa no mapa."
            });
        }

        var categoriaIds = dto.CategoriaIds
            .Distinct()
            .ToList();

        if (categoriaIds.Count == 0)
        {
            return BadRequest(new
            {
                message = "Seleciona pelo menos uma categoria."
            });
        }

        var categoriasValidas = await _context.Categorias
            .Where(c => categoriaIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (categoriasValidas.Count != categoriaIds.Count)
        {
            return BadRequest(new
            {
                message = "Uma das categorias selecionadas não é válida."
            });
        }

        var numeroParticipantes = evento.Participantes
            .Count(p => p.Estado == EstadoPedido.Aceite);

        if (dto.NumMaxParticipantes < numeroParticipantes)
        {
            return BadRequest(new
            {
                message =
                    $"A lotação não pode ser inferior aos {numeroParticipantes} participantes atuais."
            });
        }

        evento.Titulo = dto.Titulo.Trim();
        evento.Descricao = dto.Descricao.Trim();
        evento.DataHora = dto.DataHora;
        evento.DataFim = dto.DataFim;
        evento.Estado = _estadoEventoService.CalcularEstado(
            evento.DataHora,
            evento.DataFim,
            evento.Estado);

        evento.Local = dto.Local.Trim();
        evento.Morada = string.IsNullOrWhiteSpace(dto.Morada)
            ? null
            : dto.Morada.Trim();

        evento.NumMaxParticipantes = dto.NumMaxParticipantes;
        evento.IsPrivado = dto.IsPrivado;
        evento.Latitude = dto.Latitude;
        evento.Longitude = dto.Longitude;

        var categoriasParaRemover = evento.EventosCategorias
            .Where(ec => !categoriasValidas.Contains(ec.CategoriaId))
            .ToList();

        _context.EventosCategorias.RemoveRange(categoriasParaRemover);

        var categoriasAtuais = evento.EventosCategorias
            .Select(ec => ec.CategoriaId)
            .ToList();

        var categoriasParaAdicionar = categoriasValidas
            .Where(idCategoria => !categoriasAtuais.Contains(idCategoria));

        foreach (var categoriaId in categoriasParaAdicionar)
        {
            evento.EventosCategorias.Add(new EventoCategoria
            {
                EventoId = evento.Id,
                CategoriaId = categoriaId
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = evento.Id,
            message = "Evento atualizado com sucesso."
        });
    }

    [Authorize]
    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> CancelarEvento(int id)
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var evento = await _context.Eventos
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null)
        {
            return NotFound(new
            {
                message = "Evento não encontrado."
            });
        }

        if (evento.CriadorId != utilizadorId)
        {
            return Forbid();
        }

        if (evento.Estado == EstadoEvento.Cancelado)
        {
            return BadRequest(new
            {
                message = "Este evento já se encontra cancelado."
            });
        }

        if (evento.Estado == EstadoEvento.Terminado ||
            evento.DataFim <= DateTime.Now)
        {
            return BadRequest(new
            {
                message = "Não é possível cancelar um evento terminado."
            });
        }

        var participantes = await _context.Participantes
            .Where(p =>
                p.EventoId == id &&
                p.Estado == EstadoPedido.Aceite &&
                p.UtilizadorId != utilizadorId)
            .Select(p => p.UtilizadorId)
            .ToListAsync();

        var convidados = await _context.ConvitesEvento
            .Where(c =>
                c.EventoId == id &&
                c.RecetorId != utilizadorId &&
                (
                    c.Estado == EstadoPedido.Pendente ||
                    c.Estado == EstadoPedido.Aceite
                ))
            .Select(c => c.RecetorId)
            .ToListAsync();

        var destinatarios = participantes
            .Concat(convidados)
            .Distinct()
            .ToList();

        evento.Estado = EstadoEvento.Cancelado;

        await _context.SaveChangesAsync();

        var link = $"/Eventos/Details?id={evento.Id}";

        foreach (var destinatarioId in destinatarios)
        {
            await _notificacaoService.CriarAsync(
                destinatarioId,
                "Evento cancelado",
                $"O evento {evento.Titulo} foi cancelado pelo criador.",
                link);
        }

        return Ok(new
        {
            message = "O evento foi cancelado com sucesso."
        });
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> ApagarEvento(int id)
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var evento = await _context.Eventos
            .Include(e => e.EventosCategorias)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null)
        {
            return NotFound(new
            {
                message = "Evento não encontrado."
            });
        }

        if (evento.CriadorId != utilizadorId)
        {
            return Forbid();
        }

        _context.Eventos.Remove(evento);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
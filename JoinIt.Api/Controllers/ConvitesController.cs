using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Convites;
using JoinIt.Api.Enums;
using JoinIt.Api.Models;
using JoinIt.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JoinIt.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/convites")]
public class ConvitesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEstadoEventoService _estadoEventoService;
    private readonly INotificacaoService _notificacaoService;

    public ConvitesController(
        ApplicationDbContext context,
        IEstadoEventoService estadoEventoService,
        INotificacaoService notificacaoService)
    {
        _context = context;
        _estadoEventoService = estadoEventoService;
        _notificacaoService = notificacaoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConviteDto>>> GetConvites()
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        await _estadoEventoService.AtualizarEstadosAsync();

        var convites = await _context.ConvitesEvento
            .AsNoTracking()
            .Where(c => c.RecetorId == utilizadorId)
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new ConviteDto
            {
                Id = c.Id,
                EventoId = c.EventoId,
                EventoTitulo = c.Evento.Titulo,
                EventoDataHora = c.Evento.DataHora,
                EmissorId = c.EmissorId,
                EmissorNome = c.Emissor.Nome ?? string.Empty,
                Estado = c.Estado,
                CriadoEm = c.CriadoEm,
                RespondidoEm = c.RespondidoEm
            })
            .ToListAsync();

        return Ok(convites);
    }

    [HttpPost("{id:int}/aceitar")]
    public async Task<IActionResult> AceitarConvite(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var convite = await _context.ConvitesEvento
            .Include(c => c.Evento)
                .ThenInclude(e => e.Participantes)
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.RecetorId == utilizadorId);

        if (convite == null)
        {
            return NotFound(new
            {
                message = "O convite não foi encontrado."
            });
        }

        if (convite.Estado != EstadoPedido.Pendente)
        {
            return BadRequest(new
            {
                message = "Este convite já foi respondido."
            });
        }

        var evento = convite.Evento;

        if (!evento.IsPrivado)
        {
            return BadRequest(new
            {
                message = "Este convite não pertence a um evento privado."
            });
        }

        var estadoAtual = _estadoEventoService.CalcularEstado(
            evento.DataHora,
            evento.DataFim,
            evento.Estado);

        if (evento.Estado != estadoAtual)
        {
            evento.Estado = estadoAtual;
            await _context.SaveChangesAsync();
        }

        if (estadoAtual == EstadoEvento.Cancelado)
        {
            return BadRequest(new
            {
                message = "Não é possível aceitar o convite porque o evento foi cancelado."
            });
        }

        if (estadoAtual == EstadoEvento.Terminado)
        {
            return BadRequest(new
            {
                message = "Não é possível aceitar o convite porque o evento terminou."
            });
        }

        if (estadoAtual == EstadoEvento.ADecorrer ||
            evento.DataHora <= DateTime.Now)
        {
            return BadRequest(new
            {
                message = "Não é possível aceitar o convite porque o evento já começou."
            });
        }

        var participacao = evento.Participantes
            .FirstOrDefault(p =>
                p.UtilizadorId == utilizadorId);

        var jaParticipa =
            participacao?.Estado == EstadoPedido.Aceite;

        var numeroParticipantes = evento.Participantes
            .Count(p => p.Estado == EstadoPedido.Aceite);

        if (!jaParticipa &&
            numeroParticipantes >= evento.NumMaxParticipantes)
        {
            return BadRequest(new
            {
                message = "O evento já atingiu a lotação máxima."
            });
        }

        if (participacao == null)
        {
            evento.Participantes.Add(new Participante
            {
                UtilizadorId = utilizadorId,
                Estado = EstadoPedido.Aceite,
                DataPedido = DateTime.Now
            });
        }
        else
        {
            participacao.Estado = EstadoPedido.Aceite;
            participacao.DataPedido = DateTime.Now;
        }

        convite.Estado = EstadoPedido.Aceite;
        convite.RespondidoEm = DateTime.Now;

        await _context.SaveChangesAsync();

        var nomeUtilizador = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == utilizadorId)
            .Select(u => u.Nome)
            .FirstOrDefaultAsync() ?? "Um utilizador";

        await _notificacaoService.CriarAsync(
            convite.EmissorId,
            "Convite aceite",
            $"{nomeUtilizador} aceitou o convite para {evento.Titulo}.",
            $"/Eventos/Details?id={evento.Id}");

        return Ok(new
        {
            message = "Convite aceite. Entraste no evento."
        });
    }

    [HttpPost("{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarConvite(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var convite = await _context.ConvitesEvento
            .Include(c => c.Evento)
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.RecetorId == utilizadorId &&
                c.Estado == EstadoPedido.Pendente);

        if (convite == null)
        {
            return NotFound(new
            {
                message = "O convite pendente não foi encontrado."
            });
        }

        convite.Estado = EstadoPedido.Rejeitado;
        convite.RespondidoEm = DateTime.Now;

        await _context.SaveChangesAsync();

        var nomeUtilizador = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == utilizadorId)
            .Select(u => u.Nome)
            .FirstOrDefaultAsync() ?? "Um utilizador";

        await _notificacaoService.CriarAsync(
            convite.EmissorId,
            "Convite rejeitado",
            $"{nomeUtilizador} rejeitou o convite para {convite.Evento.Titulo}.",
            $"/Eventos/Details?id={convite.EventoId}");

        return Ok(new
        {
            message = "Convite rejeitado."
        });
    }
}
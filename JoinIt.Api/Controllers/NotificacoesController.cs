using System.Security.Claims;
using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Notificacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notificacoes")]
public class NotificacoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificacoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<NotificacoesResponseDto>> GetNotificacoes()
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var notificacoes = await _context.Notificacoes
            .AsNoTracking()
            .Where(n => n.UtilizadorId == utilizadorId)
            .OrderByDescending(n => n.CriadoEm)
            .Select(n => new NotificacaoDto
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensagem = n.Mensagem,
                Link = n.Link,
                Lida = n.Lida,
                CriadoEm = n.CriadoEm
            })
            .ToListAsync();

        return Ok(new NotificacoesResponseDto
        {
            NumeroNaoLidas = notificacoes.Count(n => !n.Lida),
            Notificacoes = notificacoes
        });
    }

    [HttpPost("{id:int}/ler")]
    public async Task<IActionResult> MarcarLida(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var notificacao = await _context.Notificacoes
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UtilizadorId == utilizadorId);

        if (notificacao == null)
        {
            return NotFound(new
            {
                message = "Notificação não encontrada."
            });
        }

        notificacao.Lida = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Notificação marcada como lida."
        });
    }

    [HttpPost("ler-todas")]
    public async Task<IActionResult> MarcarTodasLidas()
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var notificacoesNaoLidas = await _context.Notificacoes
            .Where(n =>
                n.UtilizadorId == utilizadorId &&
                !n.Lida)
            .ToListAsync();

        foreach (var notificacao in notificacoesNaoLidas)
        {
            notificacao.Lida = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Todas as notificações foram marcadas como lidas."
        });
    }
}
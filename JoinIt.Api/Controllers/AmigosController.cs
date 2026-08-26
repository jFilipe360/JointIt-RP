using JoinIt.Api.Data;
using JoinIt.Api.DTOs.Amigos;
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
[Route("api/amigos")]
public class AmigosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificacaoService _notificacaoService;

    public AmigosController(
        ApplicationDbContext context,
        INotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    [HttpPost("pedidos/{recetorId}")]
    public async Task<IActionResult> EnviarPedido(string recetorId)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(recetorId))
        {
            return BadRequest(new
            {
                message = "O utilizador selecionado não é válido."
            });
        }

        if (recetorId == utilizadorId)
        {
            return BadRequest(new
            {
                message = "Não podes enviar um pedido de amizade a ti próprio."
            });
        }

        var utilizadorDestino = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == recetorId);

        if (utilizadorDestino == null)
        {
            return NotFound(new
            {
                message = "Utilizador não encontrado."
            });
        }

        var amizadesExistentes = await _context.Amizades
            .Where(a =>
                (a.EmissorId == utilizadorId &&
                 a.RecetorId == recetorId) ||
                (a.EmissorId == recetorId &&
                 a.RecetorId == utilizadorId))
            .ToListAsync();

        var amizadeAceite = amizadesExistentes
            .FirstOrDefault(a => a.Estado == EstadoPedido.Aceite);

        var amizadePendente = amizadesExistentes
            .FirstOrDefault(a => a.Estado == EstadoPedido.Pendente);

        var amizadeRejeitada = amizadesExistentes
            .FirstOrDefault(a => a.Estado == EstadoPedido.Rejeitado);

        string message;

        if (amizadeAceite != null)
        {
            return BadRequest(new
            {
                message = "Já são amigos."
            });
        }

        if (amizadePendente != null)
        {
            return BadRequest(new
            {
                message = "Já existe um pedido de amizade pendente."
            });
        }

        if (amizadeRejeitada != null)
        {
            amizadeRejeitada.EmissorId = utilizadorId;
            amizadeRejeitada.RecetorId = recetorId;
            amizadeRejeitada.Estado = EstadoPedido.Pendente;
            amizadeRejeitada.CriadoEm = DateTime.Now;

            message = "Pedido de amizade enviado novamente.";
        }
        else
        {
            _context.Amizades.Add(new Amizade
            {
                EmissorId = utilizadorId,
                RecetorId = recetorId,
                Estado = EstadoPedido.Pendente,
                CriadoEm = DateTime.Now
            });

            message = "Pedido de amizade enviado.";
        }

        await _context.SaveChangesAsync();

        var nomeUtilizador = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == utilizadorId)
            .Select(u => u.Nome)
            .FirstOrDefaultAsync() ?? "Um utilizador";

        await _notificacaoService.CriarAsync(
            recetorId,
            "Novo pedido de amizade",
            $"{nomeUtilizador} enviou-te um pedido de amizade.",
            "/Friends");

        return Ok(new
        {
            message
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAmigos()
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var pedidosRecebidos = await _context.Amizades
            .AsNoTracking()
            .Where(a =>
                a.RecetorId == utilizadorId &&
                a.Estado == EstadoPedido.Pendente)
            .OrderByDescending(a => a.CriadoEm)
            .Select(a => new PedidoAmizadeDto
            {
                Id = a.Id,
                UtilizadorId = a.EmissorId,
                Nome = a.Emissor.Nome,
                FotoPerfil = a.Emissor.FotoPerfil,
                Estado = a.Estado,
                CriadoEm = a.CriadoEm
            })
            .ToListAsync();

        var pedidosEnviados = await _context.Amizades
            .AsNoTracking()
            .Where(a =>
                a.EmissorId == utilizadorId &&
                a.Estado == EstadoPedido.Pendente)
            .OrderByDescending(a => a.CriadoEm)
            .Select(a => new PedidoAmizadeDto
            {
                Id = a.Id,
                UtilizadorId = a.RecetorId,
                Nome = a.Recetor.Nome,
                FotoPerfil = a.Recetor.FotoPerfil,
                Estado = a.Estado,
                CriadoEm = a.CriadoEm
            })
            .ToListAsync();

        var amigos = await _context.Amizades
            .AsNoTracking()
            .Where(a =>
                a.Estado == EstadoPedido.Aceite &&
                (a.EmissorId == utilizadorId ||
                 a.RecetorId == utilizadorId))
            .Select(a => new AmigoDto
            {
                AmizadeId = a.Id,
                UtilizadorId = a.EmissorId == utilizadorId
                    ? a.RecetorId
                    : a.EmissorId,
                Nome = a.EmissorId == utilizadorId
                    ? a.Recetor.Nome
                    : a.Emissor.Nome,
                FotoPerfil = a.EmissorId == utilizadorId
                    ? a.Recetor.FotoPerfil
                    : a.Emissor.FotoPerfil
            })
            .OrderBy(a => a.Nome)
            .ToListAsync();

        return Ok(new
        {
            amigos,
            pedidosRecebidos,
            pedidosEnviados
        });
    }

    [HttpPost("pedidos/{id:int}/aceitar")]
    public async Task<IActionResult> AceitarPedido(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var amizade = await _context.Amizades
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.RecetorId == utilizadorId &&
                a.Estado == EstadoPedido.Pendente);

        if (amizade == null)
        {
            return NotFound(new
            {
                message = "O pedido de amizade não foi encontrado."
            });
        }

        amizade.Estado = EstadoPedido.Aceite;

        await _context.SaveChangesAsync();

        var nomeUtilizador = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == utilizadorId)
            .Select(u => u.Nome)
            .FirstOrDefaultAsync() ?? "Um utilizador";

        await _notificacaoService.CriarAsync(
            amizade.EmissorId,
            "Pedido de amizade aceite",
            $"{nomeUtilizador} aceitou o teu pedido de amizade.",
            $"/Users/Details?id={utilizadorId}");

        return Ok(new
        {
            message = "Pedido de amizade aceite."
        });
    }

    [HttpPost("pedidos/{id:int}/rejeitar")]
    public async Task<IActionResult> RejeitarPedido(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var amizade = await _context.Amizades
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.RecetorId == utilizadorId &&
                a.Estado == EstadoPedido.Pendente);

        if (amizade == null)
        {
            return NotFound(new
            {
                message = "O pedido de amizade pendente não foi encontrado."
            });
        }

        amizade.Estado = EstadoPedido.Rejeitado;

        await _context.SaveChangesAsync();

        var nomeUtilizador = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == utilizadorId)
            .Select(u => u.Nome)
            .FirstOrDefaultAsync() ?? "Um utilizador";

        await _notificacaoService.CriarAsync(
            amizade.EmissorId,
            "Pedido de amizade rejeitado",
            $"{nomeUtilizador} rejeitou o teu pedido de amizade.",
            "/Amigos");

        return Ok(new
        {
            message = "Pedido de amizade rejeitado."
        });
    }

    [HttpDelete("pedidos/{id:int}")]
    public async Task<IActionResult> CancelarPedido(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var amizade = await _context.Amizades
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.EmissorId == utilizadorId &&
                a.Estado == EstadoPedido.Pendente);

        if (amizade == null)
        {
            return NotFound(new
            {
                message = "O pedido de amizade não foi encontrado."
            });
        }

        _context.Amizades.Remove(amizade);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Pedido de amizade cancelado."
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoverAmizade(int id)
    {
        var utilizadorId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (utilizadorId == null)
        {
            return Unauthorized();
        }

        var amizade = await _context.Amizades
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.Estado == EstadoPedido.Aceite &&
                (a.EmissorId == utilizadorId ||
                 a.RecetorId == utilizadorId));

        if (amizade == null)
        {
            return NotFound(new
            {
                message = "A amizade não foi encontrada."
            });
        }

        _context.Amizades.Remove(amizade);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Amizade removida."
        });
    }
}
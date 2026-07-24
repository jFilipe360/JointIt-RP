using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Users
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser Utilizador { get; private set; } = null!;

        public IList<Evento> EventosPublicos { get; private set; } = new List<Evento>();

        public bool PerfilProprio { get; private set; }

        public bool SaoAmigos { get; private set; }

        public bool PedidoEnviado { get; private set; }

        public bool PedidoRecebido { get; private set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            string? utilizadorAtualId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorAtualId))
            {
                return Challenge();
            }

            var utilizador = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilizador is null)
            {
                return NotFound();
            }

            PerfilProprio = utilizadorAtualId == utilizador.Id;

            Utilizador = utilizador;

            if (!PerfilProprio)
            {
                var amizade = await _context.Amizades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a =>
                        (a.EmissorId == utilizadorAtualId &&
                         a.RecetorId == utilizador.Id) ||
                        (a.EmissorId == utilizador.Id &&
                         a.RecetorId == utilizadorAtualId));

                if (amizade is not null)
                {
                    SaoAmigos =
                        amizade.Estado == EstadoPedido.Aceite;

                    PedidoEnviado =
                        amizade.Estado == EstadoPedido.Pendente &&
                        amizade.EmissorId == utilizadorAtualId;

                    PedidoRecebido =
                        amizade.Estado == EstadoPedido.Pendente &&
                        amizade.RecetorId == utilizadorAtualId;
                }
            }

            EventosPublicos = await _context.Eventos
                .AsNoTracking()
                .Where(e =>
                    e.CriadorId == utilizador.Id &&
                    !e.IsPrivado)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .OrderBy(e => e.DataHora)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostEnviarPedidoAsync(
            string id)
        {
            string? utilizadorAtualId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorAtualId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (id == utilizadorAtualId)
            {
                TempData["MensagemErro"] =
                    "Não podes enviar um pedido de amizade a ti próprio.";

                return RedirectToPage("./Details", new { id });
            }

            bool utilizadorExiste = await _context.Users
                .AnyAsync(u => u.Id == id);

            if (!utilizadorExiste)
            {
                return NotFound();
            }

            var amizadeExistente = await _context.Amizades
                .FirstOrDefaultAsync(a =>
                    (a.EmissorId == utilizadorAtualId &&
                     a.RecetorId == id) ||
                    (a.EmissorId == id &&
                     a.RecetorId == utilizadorAtualId));

            if (amizadeExistente is null)
            {
                _context.Amizades.Add(new Amizade
                {
                    EmissorId = utilizadorAtualId,
                    RecetorId = id,
                    Estado = EstadoPedido.Pendente,
                    CriadoEm = DateTime.Now
                });

                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] =
                    "Pedido de amizade enviado.";
            }
            else if (amizadeExistente.Estado == EstadoPedido.Rejeitado)
            {
                // Permite enviar novamente após uma rejeição anterior.
                amizadeExistente.EmissorId = utilizadorAtualId;
                amizadeExistente.RecetorId = id;
                amizadeExistente.Estado = EstadoPedido.Pendente;
                amizadeExistente.CriadoEm = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] =
                    "Pedido de amizade enviado novamente.";
            }
            else if (amizadeExistente.Estado == EstadoPedido.Aceite)
            {
                TempData["MensagemErro"] =
                    "Já são amigos.";
            }
            else
            {
                TempData["MensagemErro"] =
                    "Já existe um pedido de amizade pendente.";
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostCancelarPedidoAsync(
            string id)
        {
            string? utilizadorAtualId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorAtualId))
            {
                return Challenge();
            }

            var amizade = await _context.Amizades
                .FirstOrDefaultAsync(a =>
                    a.EmissorId == utilizadorAtualId &&
                    a.RecetorId == id &&
                    a.Estado == EstadoPedido.Pendente);

            if (amizade is not null)
            {
                _context.Amizades.Remove(amizade);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] =
                    "Pedido de amizade cancelado.";
            }

            return RedirectToPage("./Details", new { id });
        }

    }
}
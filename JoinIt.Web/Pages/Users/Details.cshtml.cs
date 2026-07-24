using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
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
        private readonly INotificacaoService _notificacaoService;

        public DetailsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificacaoService notificacaoService)
        {
            _context = context;
            _userManager = userManager;
            _notificacaoService = notificacaoService;
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

        public async Task<IActionResult> OnPostEnviarPedidoAsync(string id)
        {
            var utilizadorAtual = await _userManager.GetUserAsync(User);

            if (utilizadorAtual is null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (id == utilizadorAtual.Id)
            {
                TempData["MensagemErro"] =
                    "Não podes enviar um pedido de amizade a ti próprio.";

                return RedirectToPage("./Details", new { id });
            }

            var utilizadorDestino = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilizadorDestino is null)
            {
                return NotFound();
            }

            var amizadeExistente = await _context.Amizades
                .FirstOrDefaultAsync(a =>
                    (a.EmissorId == utilizadorAtual.Id &&
                     a.RecetorId == id) ||
                    (a.EmissorId == id &&
                     a.RecetorId == utilizadorAtual.Id));

            bool criarNotificacao = false;

            if (amizadeExistente is null)
            {
                _context.Amizades.Add(new Amizade
                {
                    EmissorId = utilizadorAtual.Id,
                    RecetorId = id,
                    Estado = EstadoPedido.Pendente,
                    CriadoEm = DateTime.Now
                });

                criarNotificacao = true;

                TempData["MensagemSucesso"] =
                    "Pedido de amizade enviado.";
            }
            else if (amizadeExistente.Estado == EstadoPedido.Rejeitado)
            {
                amizadeExistente.EmissorId = utilizadorAtual.Id;
                amizadeExistente.RecetorId = id;
                amizadeExistente.Estado = EstadoPedido.Pendente;
                amizadeExistente.CriadoEm = DateTime.Now;

                criarNotificacao = true;

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

            if (criarNotificacao)
            {
                await _context.SaveChangesAsync();

                string link =
                    Url.Page("/Amigos/Index") ?? "/Friends";

                await _notificacaoService.CriarAsync(
                    utilizadorDestino.Id,
                    "Novo pedido de amizade",
                    $"{utilizadorAtual.Nome} enviou-te um pedido de amizade.",
                    link);
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
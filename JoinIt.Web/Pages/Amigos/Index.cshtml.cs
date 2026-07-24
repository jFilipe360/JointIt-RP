using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Friends
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificacaoService _notificacaoService;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificacaoService notificacaoService)
        {
            _context = context;
            _userManager = userManager;
            _notificacaoService = notificacaoService;
        }

        public IList<Amizade> PedidosRecebidos { get; private set; }
            = new List<Amizade>();

        public IList<Amizade> PedidosEnviados { get; private set; }
            = new List<Amizade>();

        public IList<AmigoViewModel> Amigos { get; private set; }
            = new List<AmigoViewModel>();

        public class AmigoViewModel
        {
            public int AmizadeId { get; set; }

            public ApplicationUser Utilizador { get; set; } = null!;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            await CarregarDadosAsync(utilizadorId);

            return Page();
        }

        public async Task<IActionResult> OnPostAceitarAsync(int id)
        {
            var utilizadorAtual = await _userManager.GetUserAsync(User);

            if (utilizadorAtual is null)
            {
                return Challenge();
            }

            var amizade = await _context.Amizades
                .Include(a => a.Emissor)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.RecetorId == utilizadorAtual.Id &&
                    a.Estado == EstadoPedido.Pendente);

            if (amizade is null)
            {
                TempData["MensagemErro"] =
                    "O pedido de amizade não foi encontrado.";

                return RedirectToPage();
            }

            amizade.Estado = EstadoPedido.Aceite;

            await _context.SaveChangesAsync();

            string link = Url.Page(
                "/Users/Details",
                new { id = utilizadorAtual.Id })
                ?? $"/Users/Details?id={utilizadorAtual.Id}";

            await _notificacaoService.CriarAsync(
                amizade.EmissorId,
                "Pedido de amizade aceite",
                $"{utilizadorAtual.Nome} aceitou o teu pedido de amizade.",
                link);

            TempData["MensagemSucesso"] =
                "Pedido de amizade aceite.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejeitarAsync(int id)
        {
            var utilizadorAtual = await _userManager.GetUserAsync(User);

            if (utilizadorAtual is null)
            {
                return Challenge();
            }

            var amizade = await _context.Amizades
                .Include(a => a.Emissor)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.RecetorId == utilizadorAtual.Id &&
                    a.Estado == EstadoPedido.Pendente);

            if (amizade is null)
            {
                TempData["MensagemErro"] =
                    "O pedido de amizade pendente não foi encontrado.";

                return RedirectToPage();
            }

            amizade.Estado = EstadoPedido.Rejeitado;

            await _context.SaveChangesAsync();

            string link =
                Url.Page("/Amigos/Index") ?? "/Amigos";

            await _notificacaoService.CriarAsync(
                amizade.EmissorId,
                "Pedido de amizade rejeitado",
                $"{utilizadorAtual.Nome} rejeitou o teu pedido de amizade.",
                link);

            TempData["MensagemSucesso"] =
                "Pedido de amizade rejeitado.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelarAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var amizade = await _context.Amizades
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.EmissorId == utilizadorId &&
                    a.Estado == EstadoPedido.Pendente);

            if (amizade is null)
            {
                TempData["MensagemErro"] =
                    "O pedido de amizade não foi encontrado.";

                return RedirectToPage();
            }

            _context.Amizades.Remove(amizade);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Pedido de amizade cancelado.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoverAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var amizade = await _context.Amizades
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.Estado == EstadoPedido.Aceite &&
                    (a.EmissorId == utilizadorId ||
                     a.RecetorId == utilizadorId));

            if (amizade is null)
            {
                TempData["MensagemErro"] =
                    "A amizade não foi encontrada.";

                return RedirectToPage();
            }

            _context.Amizades.Remove(amizade);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Amizade removida.";

            return RedirectToPage();
        }

        private async Task CarregarDadosAsync(string utilizadorId)
        {
            PedidosRecebidos = await _context.Amizades
                .AsNoTracking()
                .Where(a =>
                    a.RecetorId == utilizadorId &&
                    a.Estado == EstadoPedido.Pendente)
                .Include(a => a.Emissor)
                .OrderByDescending(a => a.CriadoEm)
                .ToListAsync();

            PedidosEnviados = await _context.Amizades
                .AsNoTracking()
                .Where(a =>
                    a.EmissorId == utilizadorId &&
                    a.Estado == EstadoPedido.Pendente)
                .Include(a => a.Recetor)
                .OrderByDescending(a => a.CriadoEm)
                .ToListAsync();

            var amizadesAceites = await _context.Amizades
                .AsNoTracking()
                .Where(a =>
                    a.Estado == EstadoPedido.Aceite &&
                    (a.EmissorId == utilizadorId ||
                     a.RecetorId == utilizadorId))
                .Include(a => a.Emissor)
                .Include(a => a.Recetor)
                .ToListAsync();

            Amigos = amizadesAceites
                .Select(a => new AmigoViewModel
                {
                    AmizadeId = a.Id,
                    Utilizador = a.EmissorId == utilizadorId
                        ? a.Recetor
                        : a.Emissor
                })
                .OrderBy(a => a.Utilizador.Nome)
                .ToList();
        }
    }
}
using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Invites
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<ConviteEvento> ConvitesPendentes { get; private set; }
            = new List<ConviteEvento>();

        public IList<ConviteEvento> HistoricoConvites { get; private set; }
            = new List<ConviteEvento>();

        public async Task<IActionResult> OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            await CarregarConvitesAsync(utilizadorId);

            return Page();
        }

        public async Task<IActionResult> OnPostAceitarAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var convite = await _context.ConvitesEvento
                .Include(c => c.Evento)
                    .ThenInclude(e => e.Participantes)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.RecetorId == utilizadorId);

            if (convite is null)
            {
                TempData["MensagemErro"] =
                    "O convite não foi encontrado.";

                return RedirectToPage();
            }

            if (convite.Estado != EstadoPedido.Pendente)
            {
                TempData["MensagemErro"] =
                    "Este convite já foi respondido.";

                return RedirectToPage();
            }

            Evento evento = convite.Evento;

            if (!evento.IsPrivado)
            {
                TempData["MensagemErro"] =
                    "Este convite não pertence a um evento privado.";

                return RedirectToPage();
            }

            if (evento.Estado != EstadoEvento.ParaBreve ||
                evento.DataHora <= DateTime.Now)
            {
                TempData["MensagemErro"] =
                    "Já não é possível aceitar este convite.";

                return RedirectToPage();
            }

            int numeroParticipantes = evento.Participantes.Count(
                p => p.Estado == EstadoPedido.Aceite);

            if (numeroParticipantes >= evento.NumMaxParticipantes)
            {
                TempData["MensagemErro"] =
                    "O evento já atingiu a lotação máxima.";

                return RedirectToPage();
            }

            var participacao = evento.Participantes
                .FirstOrDefault(p =>
                    p.UtilizadorId == utilizadorId);

            if (participacao is null)
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

            TempData["MensagemSucesso"] =
                "Convite aceite. Entraste no evento.";

            return RedirectToPage(
                "/Eventos/Details",
                new { id = evento.Id });
        }

        public async Task<IActionResult> OnPostRejeitarAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var convite = await _context.ConvitesEvento
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.RecetorId == utilizadorId &&
                    c.Estado == EstadoPedido.Pendente);

            if (convite is null)
            {
                TempData["MensagemErro"] =
                    "O convite pendente não foi encontrado.";

                return RedirectToPage();
            }

            convite.Estado = EstadoPedido.Rejeitado;
            convite.RespondidoEm = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Convite rejeitado.";

            return RedirectToPage();
        }

        private async Task CarregarConvitesAsync(
            string utilizadorId)
        {
            var convites = await _context.ConvitesEvento
                .AsNoTracking()
                .Where(c => c.RecetorId == utilizadorId)
                .Include(c => c.Emissor)
                .Include(c => c.Evento)
                    .ThenInclude(e => e.Criador)
                .OrderByDescending(c => c.CriadoEm)
                .ToListAsync();

            ConvitesPendentes = convites
                .Where(c => c.Estado == EstadoPedido.Pendente)
                .ToList();

            HistoricoConvites = convites
                .Where(c => c.Estado != EstadoPedido.Pendente)
                .ToList();
        }
    }
}
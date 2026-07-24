using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Notifications
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

        public IList<Notificacao> Notificacoes { get; private set; }
            = new List<Notificacao>();

        public int NumeroNaoLidas { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            await CarregarNotificacoesAsync(utilizadorId);

            return Page();
        }

        public async Task<IActionResult> OnPostMarcarLidaAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var notificacao = await _context.Notificacoes
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    n.UtilizadorId == utilizadorId);

            if (notificacao is null)
            {
                return NotFound();
            }

            notificacao.Lida = true;

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Notificação marcada como lida.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAbrirAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var notificacao = await _context.Notificacoes
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    n.UtilizadorId == utilizadorId);

            if (notificacao is null)
            {
                return NotFound();
            }

            if (!notificacao.Lida)
            {
                notificacao.Lida = true;
                await _context.SaveChangesAsync();
            }

            // Só permite redirecionamentos internos da aplicação.
            if (!string.IsNullOrWhiteSpace(notificacao.Link) &&
                Url.IsLocalUrl(notificacao.Link))
            {
                return LocalRedirect(notificacao.Link);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarcarTodasAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
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

            TempData["MensagemSucesso"] =
                "Todas as notificações foram marcadas como lidas.";

            return RedirectToPage();
        }

        private async Task CarregarNotificacoesAsync(
            string utilizadorId)
        {
            Notificacoes = await _context.Notificacoes
                .AsNoTracking()
                .Where(n => n.UtilizadorId == utilizadorId)
                .OrderByDescending(n => n.CriadoEm)
                .ToListAsync();

            NumeroNaoLidas = Notificacoes.Count(n => !n.Lida);
        }
    }
}
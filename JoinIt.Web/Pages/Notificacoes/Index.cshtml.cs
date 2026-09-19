using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Notificacoes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Notificacao> Notificacoes { get; private set; } = new List<Notificacao>();

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

        // Marca como lida uma notificação pertencente ao utilizador atual
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

            TempData["MensagemSucesso"] = "Notificação marcada como lida.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApagarAsync(int id)
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

            _context.Notificacoes.Remove(notificacao);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Notificação apagada.";

            return RedirectToPage();
        }

        // Marca a notificação como lida e abre a ligação associada, quando válida
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

        // Marca como lidas todas as notificações não lidas do utilizador
        public async Task<IActionResult> OnPostMarcarTodasAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var notificacoesNaoLidas = await _context.Notificacoes
                .Where(n =>
                    n.UtilizadorId == utilizadorId && !n.Lida)
                .ToListAsync();

            foreach (var notificacao in notificacoesNaoLidas)
            {
                notificacao.Lida = true;
            }

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Todas as notificações foram marcadas como lidas.";

            return RedirectToPage();
        }

        // Remove todas as notificações pertencentes ao utilizador atual
        public async Task<IActionResult> OnPostLimparTodasAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UtilizadorId == utilizadorId)
                .ToListAsync();

            if (notificacoes.Count > 0)
            {
                _context.Notificacoes.RemoveRange(notificacoes);

                await _context.SaveChangesAsync();
            }

            TempData["MensagemSucesso"] = "Todas as notificações foram apagadas.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarcarDropdownLidasAsync(List<int> ids)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Unauthorized();
            }

            if (ids.Count == 0)
            {
                return new JsonResult(new
                {
                    sucesso = true,
                    marcadas = 0
                });
            }

            // Marca apenas as notificações não lidas que estavam presentes no dropdown
            var notificacoes = await _context.Notificacoes
                .Where(n =>
                    n.UtilizadorId == utilizadorId &&
                    ids.Contains(n.Id) &&
                    !n.Lida)
                .ToListAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                sucesso = true,
                marcadas = notificacoes.Count
            });
        }

        // Carrega as notificações por ordem recente e calcula o número de não lidas
        private async Task CarregarNotificacoesAsync(string utilizadorId)
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
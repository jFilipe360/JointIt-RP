using JoinIt.Web.Data;
using JoinIt.Web.Models;
using JoinIt.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.ViewComponents
{
    // Carrega o número de notificações não lidas e as notificações recentes do dropdown
    public class NotificacaoCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificacaoCountViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string? utilizadorId = _userManager.GetUserId(HttpContext.User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return View(new NotificacoesDropdownViewModel());
            }

            // Calcula o total de notificações não lidas do utilizador
            int numeroNaoLidas = await _context.Notificacoes
                .CountAsync(n =>
                    n.UtilizadorId == utilizadorId &&
                    !n.Lida);

            // Carrega apenas as cinco notificações mais recentes para o dropdown
            var notificacoes = await _context.Notificacoes
                .AsNoTracking()
                .Where(n =>
                    n.UtilizadorId == utilizadorId)
                .OrderByDescending(n => n.CriadoEm)
                .Take(5)
                .ToListAsync();

            var model = new NotificacoesDropdownViewModel
            {
                NumeroNaoLidas = numeroNaoLidas,
                Notificacoes = notificacoes
            };

            return View(model);
        }
    }
}
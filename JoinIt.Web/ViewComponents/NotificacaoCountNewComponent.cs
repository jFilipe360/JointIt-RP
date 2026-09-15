using JoinIt.Web.Data;
using JoinIt.Web.Models;
using JoinIt.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.ViewComponents
{
    public class NotificacaoCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificacaoCountViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string? utilizadorId =
                _userManager.GetUserId(HttpContext.User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return View(
                    new NotificacoesDropdownViewModel());
            }

            int numeroNaoLidas = await _context.Notificacoes
                .AsNoTracking()
                .CountAsync(n =>
                    n.UtilizadorId == utilizadorId &&
                    !n.Lida);

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

            return View(numeroNaoLidas);
        }
    }
}
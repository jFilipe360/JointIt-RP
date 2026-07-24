using JoinIt.Web.Data;
using JoinIt.Web.Models;
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
                return View(0);
            }

            int numeroNaoLidas = await _context.Notificacoes
                .AsNoTracking()
                .CountAsync(n =>
                    n.UtilizadorId == utilizadorId &&
                    !n.Lida);

            return View(numeroNaoLidas);
        }
    }
}
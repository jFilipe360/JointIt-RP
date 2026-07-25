using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Perfil
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

        public ApplicationUser Utilizador { get; private set; } = null!;

        public int NumeroAmigos { get; private set; }

        public int NumeroEventosCriados { get; private set; }

        public int NumeroEventosParticipados { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var utilizador = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == utilizadorId);

            if (utilizador is null)
            {
                return NotFound();
            }

            Utilizador = utilizador;

            NumeroAmigos = await _context.Amizades
                .AsNoTracking()
                .CountAsync(a =>
                    a.Estado == EstadoPedido.Aceite &&
                    (
                        a.EmissorId == utilizadorId ||
                        a.RecetorId == utilizadorId
                    ));

            NumeroEventosCriados = await _context.Eventos
                .AsNoTracking()
                .CountAsync(e =>
                    e.CriadorId == utilizadorId);

            NumeroEventosParticipados = await _context.Participantes
                .AsNoTracking()
                .CountAsync(p =>
                    p.UtilizadorId == utilizadorId &&
                    p.Estado == EstadoPedido.Aceite &&
                    p.Evento.CriadorId != utilizadorId);

            return Page();
        }
    }
}
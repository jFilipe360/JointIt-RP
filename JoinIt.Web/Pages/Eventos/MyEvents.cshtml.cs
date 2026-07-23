using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Eventos
{
    [Authorize]
    public class MyEventsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyEventsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Evento> EventosCriados { get; set; }
            = new List<Evento>();

        public IList<Evento> EventosParticipados { get; set; }
            = new List<Evento>();

        public async Task<IActionResult> OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            EventosCriados = await _context.Eventos
                .AsNoTracking()
                .Where(e => e.CriadorId == utilizadorId)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .OrderBy(e => e.DataHora)
                .ToListAsync();

            EventosParticipados = await _context.Eventos
                .AsNoTracking()
                .Where(e =>
                    e.CriadorId != utilizadorId &&
                    e.Participantes.Any(p =>
                        p.UtilizadorId == utilizadorId &&
                        p.Estado == EstadoPedido.Aceite))
                .Include(e => e.Criador)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .OrderBy(e => e.DataHora)
                .ToListAsync();

            return Page();
        }
    }
}
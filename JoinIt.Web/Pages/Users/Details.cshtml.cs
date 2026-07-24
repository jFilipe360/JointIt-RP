using JoinIt.Web.Data;
using JoinIt.Web.Models;
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

        public DetailsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser Utilizador { get; private set; }
            = null!;

        public IList<Evento> EventosPublicos { get; private set; }
            = new List<Evento>();

        public bool PerfilProprio { get; private set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var utilizador = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilizador is null)
            {
                return NotFound();
            }

            string? utilizadorAtualId =
                _userManager.GetUserId(User);

            PerfilProprio = utilizadorAtualId == utilizador.Id;

            Utilizador = utilizador;

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
    }
}
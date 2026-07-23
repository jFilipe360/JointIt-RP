using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace JoinIt.Web.Pages.Eventos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Evento> Eventos { get; set; } = new List<Evento>();

        public async Task OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            var query = _context.Eventos
                .AsNoTracking()
                .Include(e => e.Criador)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .AsQueryable();

            if (string.IsNullOrEmpty(utilizadorId))
            {
                query = query.Where(e => !e.IsPrivado);
            }
            else
            {
                query = query.Where(e =>
                    !e.IsPrivado ||
                    e.CriadorId == utilizadorId);
            }

            Eventos = await query
                .OrderBy(e => e.DataHora)
                .ToListAsync();
        }
    }
}
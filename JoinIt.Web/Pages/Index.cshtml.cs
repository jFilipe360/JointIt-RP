using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEstadoEventoService _estadoEventoService;

        public IndexModel(
            ApplicationDbContext context,
            IEstadoEventoService estadoEventoService)
        {
            _context = context;
            _estadoEventoService = estadoEventoService;
        }
        public IList<Evento> ProximosEventos { get; private set; } = new List<Evento>();


        public async Task OnGetAsync()
        {
            await _estadoEventoService.AtualizarEstadosAsync();

            ProximosEventos = await _context.Eventos
                .AsNoTracking()
                .Where(e =>
                    !e.IsPrivado &&
                    e.Estado == EstadoEvento.ParaBreve &&
                    e.DataHora > DateTime.Now)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .OrderBy(e => e.DataHora)
                .Take(6)
                .ToListAsync();
        }
    }
}

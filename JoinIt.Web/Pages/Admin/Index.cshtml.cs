using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUtilizadores { get; private set; }
        public int TotalEventos { get; private set; }
        public int EventosAtivos { get; private set; }
        public int EventosCancelados { get; private set; }
        public int TotalParticipacoes { get; private set; }

        public async Task OnGetAsync()
        {
            TotalUtilizadores =
                await _context.Users.CountAsync();

            TotalEventos =
                await _context.Eventos.CountAsync();

            EventosAtivos =
                await _context.Eventos.CountAsync(e =>
                    e.Estado == EstadoEvento.ParaBreve ||
                    e.Estado == EstadoEvento.ADecorrer);

            EventosCancelados =
                await _context.Eventos.CountAsync(e =>
                    e.Estado == EstadoEvento.Cancelado);

            TotalParticipacoes =
                await _context.Participantes.CountAsync(p =>
                    p.Estado == EstadoPedido.Aceite);
        }
    }
}
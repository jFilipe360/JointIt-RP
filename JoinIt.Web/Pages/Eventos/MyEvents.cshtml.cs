using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
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
        private readonly IEstadoEventoService _estadoEventoService;

        public MyEventsModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEstadoEventoService estadoEventoService)
        {
            _context = context;
            _userManager = userManager;
            _estadoEventoService = estadoEventoService;
        }

        [BindProperty(SupportsGet = true)]
        public string Filtro { get; set; } = "ativos";

        public IList<Evento> EventosCriados { get; private set; }
            = new List<Evento>();

        public IList<Evento> EventosParticipados { get; private set; }
            = new List<Evento>();

        public async Task<IActionResult> OnGetAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            await _estadoEventoService.AtualizarEstadosAsync();

            var queryEventosCriados = _context.Eventos
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e => e.CriadorId == utilizadorId)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .AsQueryable();

            var queryEventosParticipados = _context.Eventos
                .AsNoTracking()
                .AsSplitQuery()
                .Where(e =>
                    e.CriadorId != utilizadorId &&
                    e.Participantes.Any(p =>
                        p.UtilizadorId == utilizadorId &&
                        p.Estado == EstadoPedido.Aceite))
                .Include(e => e.Criador)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .AsQueryable();

            queryEventosCriados =
                AplicarFiltro(queryEventosCriados);

            queryEventosParticipados =
                AplicarFiltro(queryEventosParticipados);

            EventosCriados = await Ordenar(queryEventosCriados)
                .ToListAsync();

            EventosParticipados = await Ordenar(queryEventosParticipados)
                .ToListAsync();

            return Page();
        }

        private IQueryable<Evento> AplicarFiltro(
            IQueryable<Evento> query)
        {
            return Filtro switch
            {
                "todos" => query,

                "futuros" => query.Where(e =>
                    e.Estado == EstadoEvento.ParaBreve),

                "decorrer" => query.Where(e =>
                    e.Estado == EstadoEvento.ADecorrer),

                "terminados" => query.Where(e =>
                    e.Estado == EstadoEvento.Terminado),

                "cancelados" => query.Where(e =>
                    e.Estado == EstadoEvento.Cancelado),

                _ => query.Where(e =>
                    e.Estado == EstadoEvento.ParaBreve ||
                    e.Estado == EstadoEvento.ADecorrer)
            };
        }

        private IQueryable<Evento> Ordenar(
            IQueryable<Evento> query)
        {
            if (Filtro == "terminados" ||
                Filtro == "cancelados")
            {
                return query.OrderByDescending(e => e.DataHora);
            }

            return query.OrderBy(e => e.DataHora);
        }
    }
}
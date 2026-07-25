using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using JoinIt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Eventos
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

        public IList<Evento> Eventos { get; private set; }
            = new List<Evento>();

        public IList<SelectListItem> Categorias { get; private set; }
            = new List<SelectListItem>();

        public string? ErroFiltros { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string? Pesquisa { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public EstadoEvento? Estado { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DataDe { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DataAte { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ApenasFuturos { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public bool ComLugares { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Ordenacao { get; set; } = "data-asc";

        public async Task OnGetAsync()
        {
            await _estadoEventoService.AtualizarEstadosAsync();
            await CarregarCategoriasAsync();

            DateTime agora = DateTime.Now;

            var query = _context.Eventos
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Criador)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                .Where(e => !e.IsPrivado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Pesquisa))
            {
                string pesquisa = Pesquisa.Trim();

                query = query.Where(e =>
                    e.Titulo.Contains(pesquisa) ||
                    e.Descricao.Contains(pesquisa) ||
                    e.Local.Contains(pesquisa) ||
                    (
                        e.Morada != null &&
                        e.Morada.Contains(pesquisa)
                    ) ||
                    e.EventosCategorias.Any(ec =>
                        ec.Categoria.Nome.Contains(pesquisa)
                    ));
            }

            if (CategoriaId.HasValue)
            {
                query = query.Where(e =>
                    e.EventosCategorias.Any(ec =>
                        ec.CategoriaId == CategoriaId.Value));
            }

            /*
             * Quando é escolhido um estado específico,
             * esse filtro tem prioridade sobre ApenasFuturos.
             */
            if (Estado.HasValue)
            {
                query = query.Where(e =>
                    e.Estado == Estado.Value);
            }
            else if (ApenasFuturos)
            {
                query = query.Where(e =>
                    e.DataHora > agora &&
                    e.Estado != EstadoEvento.Cancelado);
            }

            bool intervaloValido =
                !DataDe.HasValue ||
                !DataAte.HasValue ||
                DataAte.Value.Date >= DataDe.Value.Date;

            if (!intervaloValido)
            {
                ErroFiltros =
                    "A data final não pode ser anterior à data inicial.";
            }
            else
            {
                if (DataDe.HasValue)
                {
                    DateTime inicio = DataDe.Value.Date;

                    query = query.Where(e =>
                        e.DataHora >= inicio);
                }

                if (DataAte.HasValue)
                {
                    DateTime fimExclusivo =
                        DataAte.Value.Date.AddDays(1);

                    query = query.Where(e =>
                        e.DataHora < fimExclusivo);
                }
            }

            if (ComLugares)
            {
                query = query.Where(e =>
                    e.Participantes.Count(p =>
                        p.Estado == EstadoPedido.Aceite
                    ) < e.NumMaxParticipantes);
            }

            query = Ordenacao switch
            {
                "data-desc" => query
                    .OrderByDescending(e => e.DataHora),

                "nome" => query
                    .OrderBy(e => e.Titulo),

                "participantes-desc" => query
                    .OrderByDescending(e =>
                        e.Participantes.Count(p =>
                            p.Estado == EstadoPedido.Aceite))
                    .ThenBy(e => e.DataHora),

                _ => query
                    .OrderBy(e => e.DataHora)
            };

            Eventos = await query.ToListAsync();
        }

        private async Task CarregarCategoriasAsync()
        {
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nome
                })
                .ToListAsync();
        }
    }
}
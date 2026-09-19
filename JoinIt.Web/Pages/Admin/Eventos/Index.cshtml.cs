using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Eventos
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEstadoEventoService _estadoEventoService;
        private readonly INotificacaoService _notificacaoService;

        public IndexModel(ApplicationDbContext context, IEstadoEventoService estadoEventoService, INotificacaoService notificacaoService)
        {
            _context = context;
            _estadoEventoService = estadoEventoService;
            _notificacaoService = notificacaoService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Pesquisa { get; set; }

        [BindProperty(SupportsGet = true)]
        public EstadoEvento? Estado { get; set; }

        public IList<EventoAdminViewModel> Eventos { get; private set; } = new List<EventoAdminViewModel>();

        //Dados necessários para apresentar cada evento na página de administração
        public class EventoAdminViewModel
        {
            public int Id { get; set; }

            public string Titulo { get; set; } = string.Empty;

            public DateTime DataHora { get; set; }

            public DateTime DataFim { get; set; }

            public bool IsPrivado { get; set; }

            public bool IsOnline { get; set; }

            public string? Local { get; set; }

            public EstadoEvento Estado { get; set; }

            public string CriadorNome { get; set; } = string.Empty;

            public int NumeroParticipantes { get; set; }

            public int NumMaxParticipantes { get; set; }
        }

        //Atualiza os estados e carrega os eventos aplicando os filtros selecionados
        public async Task OnGetAsync()
        {
            await _estadoEventoService.AtualizarEstadosAsync();

            var query = _context.Eventos
                .AsNoTracking()
                .AsQueryable();

            //Pesquisa por título, descrição, nome do criador, local ou morada
            if (!string.IsNullOrWhiteSpace(Pesquisa))
            {
                string pesquisa = Pesquisa.Trim();

                query = query.Where(e =>
                    e.Titulo.Contains(pesquisa) ||
                    e.Descricao.Contains(pesquisa) ||
                    e.Criador.Nome.Contains(pesquisa) ||
                    (
                        e.Local != null && e.Local.Contains(pesquisa)
                    ) ||
                    (
                        e.Morada != null && e.Morada.Contains(pesquisa)
                    ));
            }

            //Filtro por estado do evento
            if (Estado.HasValue)
            {
                query = query.Where(e => e.Estado == Estado.Value);
            }

            Eventos = await query
                .OrderByDescending(e => e.DataHora)
                .Select(e => new EventoAdminViewModel
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    DataHora = e.DataHora,
                    DataFim = e.DataFim,
                    IsPrivado = e.IsPrivado,
                    IsOnline = e.IsOnline,
                    Local = e.Local,
                    Estado = e.Estado,
                    CriadorNome = e.Criador.Nome,
                    NumeroParticipantes =
                        e.Participantes.Count(p => p.Estado == EstadoPedido.Aceite),
                    NumMaxParticipantes =
                        e.NumMaxParticipantes
                })
                .ToListAsync();
        }

        //Cancela um evento e notifica os participantes
        public async Task<IActionResult> OnPostCancelarAsync(int id)
        {
            await _estadoEventoService.AtualizarEstadosAsync();

            var evento = await _context.Eventos .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            if (evento.Estado == EstadoEvento.Cancelado)
            {
                TempData["MensagemErro"] = "O evento já se encontra cancelado.";

                return RedirectToPage();
            }

            //Eventos terminados não podem ser cancelados
            if (evento.Estado == EstadoEvento.Terminado || evento.DataFim <= DateTime.Now)
            {
                TempData["MensagemErro"] = "Não é possível cancelar um evento terminado.";

                return RedirectToPage();
            }

            //Reúne participantes e convidados do evento para notificação
            var participantes = await _context.Participantes
                .Where(p =>
                    p.EventoId == id && p.Estado == EstadoPedido.Aceite)
                .Select(p => p.UtilizadorId)
                .ToListAsync();

            var convidados = await _context.ConvitesEvento
                .Where(c =>
                    c.EventoId == id &&
                    (
                        c.Estado == EstadoPedido.Pendente || c.Estado == EstadoPedido.Aceite
                    ))
                .Select(c => c.RecetorId)
                .ToListAsync();

            //Remove duplicados e exclui o criador do evento da lista de destinatários
            var destinatarios = participantes
                .Concat(convidados)
                .Where(utilizadorId => utilizadorId != evento.CriadorId)
                .Distinct()
                .ToList();

            //Guarda o cancelamento do evento antes de enviar as notificações
            evento.Estado = EstadoEvento.Cancelado;

            await _context.SaveChangesAsync();

            string link = $"/Eventos/Details?id={evento.Id}";

            //Notifica participantes e convidados do evento sobre o cancelamento
            foreach (string destinatarioId in destinatarios)
            {
                await _notificacaoService.CriarAsync(
                    destinatarioId,
                    "Evento cancelado",
                    $"O evento {evento.Titulo} foi cancelado pela administração.",
                    link);
            }

            //O criador do evento recebe uma notificação específica
            await _notificacaoService.CriarAsync(
                evento.CriadorId,
                "Evento cancelado",
                $"O teu evento {evento.Titulo} foi cancelado pela administração.",
                link);

            TempData["MensagemSucesso"] = "O evento foi cancelado.";

            return RedirectToPage();
        }

        //Remove definitivamente um evento da base de dados
        public async Task<IActionResult> OnPostApagarAsync( int id)
        {
            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            _context.Eventos.Remove(evento);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "O evento foi apagado definitivamente.";

            return RedirectToPage();
        }
    }
}
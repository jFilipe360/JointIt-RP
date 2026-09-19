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
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEstadoEventoService _estadoEventoService;

        public ChatModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IEstadoEventoService estadoEventoService)
        {
            _context = context;
            _userManager = userManager;
            _estadoEventoService = estadoEventoService;
        }

        public Evento Evento { get; private set; } = null!;

        public IList<MensagemEvento> Mensagens { get; private set; } = new List<MensagemEvento>();

        public string UtilizadorAtualId { get; private set; } = string.Empty;

        public bool PodeEnviar { get; private set; }

        //Valida o acesso ao chat e carrega as mensagens do evento
        public async Task<IActionResult> OnGetAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(utilizadorId))
            {
                return Challenge();
            }

            await _estadoEventoService.AtualizarEstadosAsync();

            var evento = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Criador)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            //Apenas o criador do evento ou um participante aceite pode aceder ao chat
            bool participaNoEvento = await _context.Participantes
                .AsNoTracking()
                .AnyAsync(p =>
                    p.EventoId == id &&
                    p.UtilizadorId == utilizadorId &&
                    p.Estado == EstadoPedido.Aceite);

            bool podeAceder = evento.CriadorId == utilizadorId || participaNoEvento;

            if (!podeAceder)
            {
                return Forbid();
            }

            Evento = evento;
            UtilizadorAtualId = utilizadorId;

            //O histórico pode ser consultado, mas só eventos ativos permiteem enviar mensagens
            PodeEnviar = evento.Estado == EstadoEvento.ParaBreve || evento.Estado == EstadoEvento.ADecorrer;

            //Carrega as 50 mensagens mais recentes do evento, ordenadas por data de envio
            var mensagensRecentes = await _context.MensagensEvento
                .AsNoTracking()
                .Where(m => m.EventoId == id)
                .Include(m => m.Utilizador)
                .OrderByDescending(m => m.EnviadaEm)
                .Take(50)
                .ToListAsync();

            Mensagens = mensagensRecentes.OrderBy(m => m.EnviadaEm).ToList();

            return Page();
        }
    }
}
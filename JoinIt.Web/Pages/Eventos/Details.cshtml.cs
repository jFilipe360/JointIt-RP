using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Eventos
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel( ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Evento Evento { get; private set; } = null!;
        public bool PodeGerir { get; private set; }
        public bool PodeParticipar { get; private set; }
        public bool EventoCheio { get; private set; }
        public int NumeroParticipantes { get; private set; }
        public EstadoPedido? EstadoParticipacaoAtual { get; private set; }
        public bool EstaAParticipar => EstadoParticipacaoAtual == EstadoPedido.Aceite;

        public bool TemConvitePendente { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var resultado = await CarregarPaginaAsync(id);

            if (resultado is not null)
            {
                return resultado;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostParticiparAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .Include(e => e.Participantes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            if (evento.CriadorId == utilizadorId)
            {
                return RedirectToPage("./Details", new { id });
            }

            // Os eventos privados serão tratados através de convites.
            if (evento.IsPrivado)
            {
                return Forbid();
            }

            if (evento.Estado != EstadoEvento.ParaBreve ||
                evento.DataHora <= DateTime.Now)
            {
                TempData["MensagemErro"] =
                    "Já não é possível participar neste evento.";

                return RedirectToPage("./Details", new { id });
            }

            int numeroParticipantes = evento.Participantes.Count(
                p => p.Estado == EstadoPedido.Aceite);

            if (numeroParticipantes >= evento.NumMaxParticipantes)
            {
                TempData["MensagemErro"] =
                    "O evento já atingiu a lotação máxima.";

                return RedirectToPage("./Details", new { id });
            }

            var participacao = evento.Participantes
                .FirstOrDefault(
                    p => p.UtilizadorId == utilizadorId);

            if (participacao is null)
            {
                evento.Participantes.Add(new Participante
                {
                    UtilizadorId = utilizadorId,
                    Estado = EstadoPedido.Aceite,
                    DataPedido = DateTime.Now
                });
            }
            else
            {
                participacao.Estado = EstadoPedido.Aceite;
                participacao.DataPedido = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Entraste no evento com sucesso.";

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostSairAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            // O criador não pode abandonar o próprio evento.
            if (evento.CriadorId == utilizadorId)
            {
                return Forbid();
            }

            var participacao = await _context.Participantes
                .FirstOrDefaultAsync(p =>
                    p.EventoId == id &&
                    p.UtilizadorId == utilizadorId);

            if (participacao is not null)
            {
                _context.Participantes.Remove(participacao);
            }

            // Ao sair de um evento privado, o convite aceite é removido.
            if (evento.IsPrivado)
            {
                var convite = await _context.ConvitesEvento
                    .FirstOrDefaultAsync(c =>
                        c.EventoId == id &&
                        c.RecetorId == utilizadorId &&
                        c.Estado == EstadoPedido.Aceite);

                if (convite is not null)
                {
                    _context.ConvitesEvento.Remove(convite);
                }
            }

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Saíste do evento.";

            if (evento.IsPrivado)
            {
                return RedirectToPage("./MyEvents");
            }

            return RedirectToPage("./Details", new { id });
        }

        private async Task<IActionResult?> CarregarPaginaAsync(int id)
        {
            var evento = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Criador)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .Include(e => e.Participantes)
                    .ThenInclude(p => p.Utilizador)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            string? utilizadorId = _userManager.GetUserId(User);

            // Por enquanto, os eventos privados só são visíveis ao criador.
            if (evento.IsPrivado && evento.CriadorId != utilizadorId)
            {
                if (string.IsNullOrEmpty(utilizadorId))
                {
                    return Challenge();
                }

                bool participaNoEvento = evento.Participantes.Any(p =>
                    p.UtilizadorId == utilizadorId &&
                    p.Estado == EstadoPedido.Aceite);

                bool temConvite = await _context.ConvitesEvento
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.EventoId == evento.Id &&
                        c.RecetorId == utilizadorId &&
                        (
                            c.Estado == EstadoPedido.Pendente ||
                            c.Estado == EstadoPedido.Aceite
                        ));

                if (!participaNoEvento && !temConvite)
                {
                    return Forbid();
                }
            }

            Evento = evento;

            PodeGerir = evento.CriadorId == utilizadorId;

            NumeroParticipantes = evento.Participantes.Count(
                p => p.Estado == EstadoPedido.Aceite);

            EventoCheio =
                NumeroParticipantes >= evento.NumMaxParticipantes;

            var participacaoAtual = evento.Participantes
                .FirstOrDefault(
                    p => p.UtilizadorId == utilizadorId);

            EstadoParticipacaoAtual =
                participacaoAtual?.Estado;

            if (!string.IsNullOrEmpty(utilizadorId))
            {
                TemConvitePendente = await _context.ConvitesEvento
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.EventoId == evento.Id &&
                        c.RecetorId == utilizadorId &&
                        c.Estado == EstadoPedido.Pendente);
            }

            PodeParticipar =
                User.Identity?.IsAuthenticated == true &&
                !PodeGerir &&
                !evento.IsPrivado &&
                !EstaAParticipar &&
                !EventoCheio &&
                evento.Estado == EstadoEvento.ParaBreve &&
                evento.DataHora > DateTime.Now;

            return null;
        }
    }
}
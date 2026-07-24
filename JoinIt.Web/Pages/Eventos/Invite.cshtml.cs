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
    public class InviteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InviteModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Evento Evento { get; private set; } = null!;

        public IList<AmigoConviteViewModel> Amigos { get; private set; }
            = new List<AmigoConviteViewModel>();

        public int NumeroParticipantes { get; private set; }

        public int LugaresDisponiveis =>
            Math.Max(
                0,
                Evento.NumMaxParticipantes - NumeroParticipantes);

        public class AmigoConviteViewModel
        {
            public ApplicationUser Utilizador { get; set; } = null!;

            public int? ConviteId { get; set; }

            public EstadoPedido? EstadoConvite { get; set; }

            public bool JaParticipa { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var resultado = await CarregarPaginaAsync(
                id,
                utilizadorId);

            if (resultado is not null)
            {
                return resultado;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEnviarAsync(
            int id,
            string recetorId)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(recetorId) ||
                recetorId == utilizadorId)
            {
                TempData["MensagemErro"] =
                    "O utilizador selecionado não é válido.";

                return RedirectToPage(new { id });
            }

            var evento = await _context.Eventos
                .Include(e => e.Participantes)
                .Include(e => e.Convites)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            if (!evento.IsPrivado)
            {
                TempData["MensagemErro"] =
                    "Só é possível enviar convites para eventos privados.";

                return RedirectToPage(
                    "./Details",
                    new { id });
            }

            if (evento.Estado != EstadoEvento.ParaBreve ||
                evento.DataHora <= DateTime.Now)
            {
                TempData["MensagemErro"] =
                    "Já não é possível enviar convites para este evento.";

                return RedirectToPage(new { id });
            }

            int numeroParticipantes = evento.Participantes.Count(
                p => p.Estado == EstadoPedido.Aceite);

            if (numeroParticipantes >=
                evento.NumMaxParticipantes)
            {
                TempData["MensagemErro"] =
                    "O evento já atingiu a lotação máxima.";

                return RedirectToPage(new { id });
            }

            bool saoAmigos = await _context.Amizades
                .AnyAsync(a =>
                    a.Estado == EstadoPedido.Aceite &&
                    (
                        (a.EmissorId == utilizadorId &&
                         a.RecetorId == recetorId) ||
                        (a.EmissorId == recetorId &&
                         a.RecetorId == utilizadorId)
                    ));

            if (!saoAmigos)
            {
                TempData["MensagemErro"] =
                    "Só podes convidar utilizadores que sejam teus amigos.";

                return RedirectToPage(new { id });
            }

            bool jaParticipa = evento.Participantes.Any(p =>
                p.UtilizadorId == recetorId &&
                p.Estado == EstadoPedido.Aceite);

            if (jaParticipa)
            {
                TempData["MensagemErro"] =
                    "Este utilizador já participa no evento.";

                return RedirectToPage(new { id });
            }

            var convite = evento.Convites
                .FirstOrDefault(c =>
                    c.RecetorId == recetorId);

            if (convite is null)
            {
                evento.Convites.Add(new ConviteEvento
                {
                    EmissorId = utilizadorId,
                    RecetorId = recetorId,
                    Estado = EstadoPedido.Pendente,
                    CriadoEm = DateTime.Now
                });

                TempData["MensagemSucesso"] =
                    "Convite enviado com sucesso.";
            }
            else if (convite.Estado == EstadoPedido.Rejeitado)
            {
                convite.EmissorId = utilizadorId;
                convite.Estado = EstadoPedido.Pendente;
                convite.CriadoEm = DateTime.Now;
                convite.RespondidoEm = null;

                TempData["MensagemSucesso"] =
                    "Convite enviado novamente.";
            }
            else if (convite.Estado == EstadoPedido.Pendente)
            {
                TempData["MensagemErro"] =
                    "Já existe um convite pendente para este utilizador.";

                return RedirectToPage(new { id });
            }
            else
            {
                TempData["MensagemErro"] =
                    "Este utilizador já aceitou o convite.";

                return RedirectToPage(new { id });
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCancelarAsync(
            int id,
            int conviteId)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            var convite = await _context.ConvitesEvento
                .FirstOrDefaultAsync(c =>
                    c.Id == conviteId &&
                    c.EventoId == id &&
                    c.EmissorId == utilizadorId &&
                    c.Estado == EstadoPedido.Pendente);

            if (convite is null)
            {
                TempData["MensagemErro"] =
                    "O convite pendente não foi encontrado.";

                return RedirectToPage(new { id });
            }

            _context.ConvitesEvento.Remove(convite);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Convite cancelado.";

            return RedirectToPage(new { id });
        }

        private async Task<IActionResult?> CarregarPaginaAsync(
            int id,
            string utilizadorId)
        {
            var evento = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Participantes)
                .Include(e => e.Convites)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            if (!evento.IsPrivado)
            {
                TempData["MensagemErro"] =
                    "Os convites só estão disponíveis em eventos privados.";

                return RedirectToPage(
                    "./Details",
                    new { id });
            }

            Evento = evento;

            NumeroParticipantes = evento.Participantes.Count(
                p => p.Estado == EstadoPedido.Aceite);

            var amizades = await _context.Amizades
                .AsNoTracking()
                .Where(a =>
                    a.Estado == EstadoPedido.Aceite &&
                    (
                        a.EmissorId == utilizadorId ||
                        a.RecetorId == utilizadorId
                    ))
                .Include(a => a.Emissor)
                .Include(a => a.Recetor)
                .ToListAsync();

            var utilizadoresAmigos = amizades
                .Select(a =>
                    a.EmissorId == utilizadorId
                        ? a.Recetor
                        : a.Emissor)
                .OrderBy(u => u.Nome)
                .ToList();

            Amigos = utilizadoresAmigos
                .Select(amigo =>
                {
                    var convite = evento.Convites
                        .FirstOrDefault(c =>
                            c.RecetorId == amigo.Id);

                    bool jaParticipa =
                        evento.Participantes.Any(p =>
                            p.UtilizadorId == amigo.Id &&
                            p.Estado == EstadoPedido.Aceite);

                    return new AmigoConviteViewModel
                    {
                        Utilizador = amigo,
                        ConviteId = convite?.Id,
                        EstadoConvite = convite?.Estado,
                        JaParticipa = jaParticipa
                    };
                })
                .ToList();

            return null;
        }
    }
}
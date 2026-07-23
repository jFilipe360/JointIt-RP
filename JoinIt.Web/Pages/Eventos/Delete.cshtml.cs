using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Eventos
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Evento Evento { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Criador)
                .Include(e => e.EventosCategorias)
                    .ThenInclude(ec => ec.Categoria)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            // Apenas o criador pode apagar o evento.
            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            Evento = evento;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .Include(e => e.EventosCategorias)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            // A autorização é novamente verificada no POST.
            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "O evento foi apagado com sucesso.";

            return RedirectToPage("./Index");
        }
    }
}
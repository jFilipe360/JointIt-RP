using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Categorias
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Categoria Categoria { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var categoria = await _context
                .Set<Categoria>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            Categoria = categoria;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var categoria = await _context
                .Set<Categoria>()
                .Include(c => c.EventosCategorias)
                .FirstOrDefaultAsync(c => c.Id == Categoria.Id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (categoria.EventosCategorias.Count > 0)
            {
                TempData["MensagemErro"] =
                    "Não é possível apagar esta categoria porque está associada a eventos.";

                return RedirectToPage("./Index");
            }

            _context.Set<Categoria>().Remove(categoria);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Categoria apagada com sucesso.";

            return RedirectToPage("./Index");
        }
    }
}
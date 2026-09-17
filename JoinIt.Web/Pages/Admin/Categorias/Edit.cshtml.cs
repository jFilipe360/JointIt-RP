using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Categorias
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Categoria Categoria { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Categoria = await _context
                .Set<Categoria>()
                .FindAsync(id);

            if (Categoria == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categoria.Nome = Categoria.Nome.Trim();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            bool existe = await _context
                .Set<Categoria>()
                .AnyAsync(c =>
                    c.Id != Categoria.Id &&
                    c.Nome == Categoria.Nome);

            if (existe)
            {
                ModelState.AddModelError(
                    "Categoria.Nome",
                    "Já existe uma categoria com este nome.");

                return Page();
            }

            var categoriaDb = await _context
                .Set<Categoria>()
                .FindAsync(Categoria.Id);

            if (categoriaDb == null)
            {
                return NotFound();
            }

            categoriaDb.Nome = Categoria.Nome;

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Categoria atualizada com sucesso.";

            return RedirectToPage("./Index");
        }
    }
}
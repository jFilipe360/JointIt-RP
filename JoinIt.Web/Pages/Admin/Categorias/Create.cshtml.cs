using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Categorias
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Categoria Categoria { get; set; } = new();

        public void OnGet()
        {
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
                .AnyAsync(c => c.Nome == Categoria.Nome);

            if (existe)
            {
                ModelState.AddModelError(
                    "Categoria.Nome",
                    "Já existe uma categoria com este nome.");

                return Page();
            }

            _context.Set<Categoria>().Add(Categoria);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "Categoria criada com sucesso.";

            return RedirectToPage("./Index");
        }
    }
}
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

        //Carrega a categoria para edição
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            Categoria = categoria;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //Normaliza o nome antes da validação
            Categoria.Nome = Categoria.Nome.Trim();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Impede nomes repetidos noutras categorias
            bool existe = await _context.Categorias
                .AnyAsync(c =>
                    c.Id != Categoria.Id &&
                    c.Nome == Categoria.Nome);

            if (existe)
            {
                ModelState.AddModelError("Categoria.Nome", "Já existe uma categoria com este nome.");

                return Page();
            }

            var categoriaDb = await _context.Categorias
                .FindAsync(Categoria.Id);

            if (categoriaDb == null)
            {
                return NotFound();
            }

            categoriaDb.Nome = Categoria.Nome;

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Categoria atualizada com sucesso.";

            return RedirectToPage("./Index");
        }
    }
}
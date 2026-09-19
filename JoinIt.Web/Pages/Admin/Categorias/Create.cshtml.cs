using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Categorias
{
    //Permite ao admin criar uma nova categoria
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
            //Remove espaços em branco desnecessários antes da validação e gravação
            Categoria.Nome = Categoria.Nome.Trim();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Impede a criação de categorias com nomes duplicados
            bool existe = await _context.Categorias.AnyAsync(c => c.Nome == Categoria.Nome);

            if (existe)
            {
                ModelState.AddModelError("Categoria.Nome","Já existe uma categoria com este nome.");
                return Page();
            }

            //Guarda a nova categoria na base de dados
            _context.Categorias.Add(Categoria);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Categoria criada com sucesso.";

            return RedirectToPage("./Index");
        }
    }
}
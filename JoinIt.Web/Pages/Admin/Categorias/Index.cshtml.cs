using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Categorias
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Categoria> Categorias { get; private set; } = new List<Categoria>();

        //Carrega as categorias ordenadas pelo nome
        public async Task OnGetAsync()
        {
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }
    }
}
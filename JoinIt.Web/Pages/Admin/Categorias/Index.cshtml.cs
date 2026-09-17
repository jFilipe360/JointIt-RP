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

        public IList<Categoria> Categorias { get; private set; }
            = new List<Categoria>();

        public async Task OnGetAsync()
        {
            Categorias = await _context
                .Set<Categoria>()
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }
    }
}
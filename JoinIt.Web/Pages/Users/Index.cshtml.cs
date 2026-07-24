using JoinIt.Web.Data;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Users
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<ApplicationUser> Utilizadores { get; set; }
            = new List<ApplicationUser>();

        public async Task OnGetAsync()
        {
            string? utilizadorAtualId =
                _userManager.GetUserId(User);

            Utilizadores = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id != utilizadorAtualId)
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }
    }
}
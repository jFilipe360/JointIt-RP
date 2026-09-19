using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JoinIt.Web.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string? Pesquisa { get; set; }

        public IList<UtilizadorAdminViewModel> Utilizadores { get; private set; }
            = new List<UtilizadorAdminViewModel>();

        //Dados necessários para exibir a lista de utilizadores na página
        public class UtilizadorAdminViewModel
        {
            public string Id { get; set; } = string.Empty;

            public string Nome { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string? FotoPerfil { get; set; }

            public DateTime CriadoEm { get; set; }

            public bool IsAdmin { get; set; }
        }

        public async Task OnGetAsync()
        {
            await CarregarUtilizadoresAsync();
        }

        //Atribui a role de administrador a um utilizador
        public async Task<IActionResult> OnPostTornarAdminAsync( string id)
        {
            ApplicationUser? utilizador = await _userManager.FindByIdAsync(id);

            if (utilizador is null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync( utilizador, "Admin"))
            {
                IdentityResult resultado = await _userManager.AddToRoleAsync(utilizador, "Admin");

                if (!resultado.Succeeded)
                {
                    TempData["MensagemErro"] = "Não foi possível atribuir a role Admin.";

                    return RedirectToPage();
                }
            }

            TempData["MensagemSucesso"] = $"{utilizador.Nome} é agora administrador.";

            return RedirectToPage();
        }

        // Remove a role de administrador de um utilizador, impedindo que um utilizador remova a sua própria role
        public async Task<IActionResult> OnPostRemoverAdminAsync( string id)
        {
            string? utilizadorAtualId = _userManager.GetUserId(User);

            //Evita que o administrador perca o próprio acesso ao painel
            if (id == utilizadorAtualId)
            {
                TempData["MensagemErro"] = "Não podes remover a tua própria role de administrador.";
                return RedirectToPage();
            }

            ApplicationUser? utilizador = await _userManager.FindByIdAsync(id);

            if (utilizador is null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync( utilizador, "Admin"))
            {
                IdentityResult resultado = await _userManager.RemoveFromRoleAsync(utilizador,"Admin");

                if (!resultado.Succeeded)
                {
                    TempData["MensagemErro"] = "Não foi possível remover a role Admin.";

                    return RedirectToPage();
                }
            }

            TempData["MensagemSucesso"] = $"{utilizador.Nome} deixou de ser administrador.";

            return RedirectToPage();
        }

        //Carrega os utilizadores, aplica a pesquisa e identifica os administradores
        private async Task CarregarUtilizadoresAsync()
        {
            var query = _userManager.Users.AsNoTracking();

            //Permite pesquisar por nome ou email
            if (!string.IsNullOrWhiteSpace(Pesquisa))
            {
                string pesquisa = Pesquisa.Trim();

                query = query.Where(u =>
                    u.Nome.Contains(pesquisa) ||
                    (
                        u.Email != null && u.Email.Contains(pesquisa)
                    ));
            }

            var utilizadores = await query.OrderBy(u => u.Nome).ToListAsync();

            var resultado = new List<UtilizadorAdminViewModel>();

            //Verifica se cada utilizador é administrador
            foreach (ApplicationUser utilizador in utilizadores)
            {
                resultado.Add(new UtilizadorAdminViewModel
                {
                    Id = utilizador.Id,
                    Nome = utilizador.Nome,
                    Email = utilizador.Email ?? string.Empty,
                    FotoPerfil = utilizador.FotoPerfil,
                    CriadoEm = utilizador.CriadoEm,
                    IsAdmin = await _userManager.IsInRoleAsync(utilizador, "Admin")
                });
            }

            Utilizadores = resultado;
        }
    }
}
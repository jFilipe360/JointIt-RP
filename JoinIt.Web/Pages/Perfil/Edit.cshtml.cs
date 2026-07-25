using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Pages.Perfil
{
    [Authorize]
    public class EditModel : PageModel
    {
        private const long TamanhoMaximoFoto = 2 * 1024 * 1024;

        private static readonly HashSet<string> ExtensoesPermitidas =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public EditModel(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public PerfilInputModel Input { get; set; } = new();

        public string? FotoAtual { get; private set; }

        public class PerfilInputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório.")]
            [StringLength(
                100,
                ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
            [Display(Name = "Nome")]
            public string Nome { get; set; } = string.Empty;

            [Display(Name = "Nova fotografia")]
            public IFormFile? Foto { get; set; }

            [Display(Name = "Remover fotografia atual")]
            public bool RemoverFoto { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var utilizador = await _userManager.GetUserAsync(User);

            if (utilizador is null)
            {
                return Challenge();
            }

            Input.Nome = utilizador.Nome;
            FotoAtual = utilizador.FotoPerfil;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var utilizador = await _userManager.GetUserAsync(User);

            if (utilizador is null)
            {
                return Challenge();
            }

            FotoAtual = utilizador.FotoPerfil;

            string nome = Input.Nome.Trim();

            if (string.IsNullOrWhiteSpace(nome))
            {
                ModelState.AddModelError(
                    "Input.Nome",
                    "O nome é obrigatório.");
            }

            if (Input.Foto is not null)
            {
                ValidarFoto(Input.Foto);
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string? caminhoFotoAntiga = utilizador.FotoPerfil;
            string? novoCaminhoFoto = caminhoFotoAntiga;
            string? novoCaminhoFisico = null;

            // Uma nova fotografia tem prioridade sobre a remoção.
            if (Input.Foto is not null)
            {
                (novoCaminhoFoto, novoCaminhoFisico) =
                    await GuardarFotoAsync(Input.Foto);
            }
            else if (Input.RemoverFoto)
            {
                novoCaminhoFoto = null;
            }

            utilizador.Nome = nome;
            utilizador.FotoPerfil = novoCaminhoFoto;

            var resultado = await _userManager.UpdateAsync(utilizador);

            if (!resultado.Succeeded)
            {
                // Remove a nova imagem caso a atualização falhe.
                if (!string.IsNullOrWhiteSpace(novoCaminhoFisico) &&
                    System.IO.File.Exists(novoCaminhoFisico))
                {
                    System.IO.File.Delete(novoCaminhoFisico);
                }

                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description);
                }

                FotoAtual = caminhoFotoAntiga;

                return Page();
            }

            if (caminhoFotoAntiga != novoCaminhoFoto)
            {
                ApagarFoto(caminhoFotoAntiga);
            }

            TempData["MensagemSucesso"] =
                "O perfil foi atualizado com sucesso.";

            return RedirectToPage("./Index");
        }

        private void ValidarFoto(IFormFile foto)
        {
            if (foto.Length == 0)
            {
                ModelState.AddModelError(
                    "Input.Foto",
                    "O ficheiro selecionado está vazio.");

                return;
            }

            if (foto.Length > TamanhoMaximoFoto)
            {
                ModelState.AddModelError(
                    "Input.Foto",
                    "A fotografia não pode ultrapassar 2 MB.");
            }

            string extensao = Path.GetExtension(foto.FileName);

            if (string.IsNullOrWhiteSpace(extensao) ||
                !ExtensoesPermitidas.Contains(extensao))
            {
                ModelState.AddModelError(
                    "Input.Foto",
                    "Seleciona uma imagem JPG, JPEG, PNG ou WEBP.");
            }
        }

        private async Task<(string CaminhoRelativo, string CaminhoFisico)>
            GuardarFotoAsync(IFormFile foto)
        {
            string pasta = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "perfis");

            Directory.CreateDirectory(pasta);

            string extensao = Path
                .GetExtension(foto.FileName)
                .ToLowerInvariant();

            string nomeFicheiro =
                $"{Guid.NewGuid():N}{extensao}";

            string caminhoFisico = Path.Combine(
                pasta,
                nomeFicheiro);

            await using var stream =
                new FileStream(
                    caminhoFisico,
                    FileMode.CreateNew);

            await foto.CopyToAsync(stream);

            string caminhoRelativo =
                $"/uploads/perfis/{nomeFicheiro}";

            return (caminhoRelativo, caminhoFisico);
        }

        private void ApagarFoto(string? caminhoRelativo)
        {
            if (string.IsNullOrWhiteSpace(caminhoRelativo))
            {
                return;
            }

            const string pastaPermitida = "/uploads/perfis/";

            if (!caminhoRelativo.StartsWith(
                    pastaPermitida,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string nomeFicheiro =
                Path.GetFileName(caminhoRelativo);

            if (string.IsNullOrWhiteSpace(nomeFicheiro))
            {
                return;
            }

            string caminhoFisico = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "perfis",
                nomeFicheiro);

            if (System.IO.File.Exists(caminhoFisico))
            {
                System.IO.File.Delete(caminhoFisico);
            }
        }
    }
}
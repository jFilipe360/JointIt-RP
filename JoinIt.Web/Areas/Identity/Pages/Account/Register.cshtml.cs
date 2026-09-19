// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace JoinIt.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório.")]
            [StringLength(
                100,
                ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
            [Display(Name = "Nome")]
            public string Nome { get; set; }

            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Introduz um email válido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
            [StringLength(
                100,
                MinimumLength = 6,
                ErrorMessage =
                    "A palavra-passe deve ter entre 6 e 100 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-passe")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Confirma a palavra-passe.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar palavra-passe")]
            [Compare(
                nameof(Password),
                ErrorMessage = "As palavras-passe não coincidem.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync(
            string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.Nome = Input.Nome.Trim();
                user.CriadoEm = DateTime.Now;

                await _userStore.SetUserNameAsync(
                    user,
                    Input.Email.Trim(),
                    CancellationToken.None);

                await _emailStore.SetEmailAsync(
                    user,
                    Input.Email.Trim(),
                    CancellationToken.None);

                var result = await _userManager.CreateAsync(
                    user,
                    Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "Foi criada uma nova conta de utilizador.");

                    await _signInManager.SignInAsync(
                        user,
                        isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator
                    .CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Não foi possível criar uma instância de " +
                    $"'{nameof(ApplicationUser)}'.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException(
                    "O armazenamento de utilizadores deve suportar email.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
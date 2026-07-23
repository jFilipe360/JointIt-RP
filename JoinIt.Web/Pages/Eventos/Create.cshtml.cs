using JoinIt.Web.Data;
using JoinIt.Web.Enums;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Pages.Eventos
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public EventoInputModel Input { get; set; } = new();

        public IList<SelectListItem> Categorias { get; set; }
            = new List<SelectListItem>();

        public class EventoInputModel
        {
            [Required(ErrorMessage = "O título é obrigatório.")]
            [StringLength(
                100,
                ErrorMessage = "O título não pode ultrapassar 100 caracteres.")]
            [Display(Name = "Título")]
            public string Titulo { get; set; } = string.Empty;

            [Required(ErrorMessage = "A descrição é obrigatória.")]
            [StringLength(
                1000,
                ErrorMessage = "A descrição não pode ultrapassar 1000 caracteres.")]
            [Display(Name = "Descrição")]
            public string Descricao { get; set; } = string.Empty;

            [Required(ErrorMessage = "A data e hora são obrigatórias.")]
            [Display(Name = "Data e hora")]
            public DateTime DataHora { get; set; }
                = DateTime.Now.AddDays(1);

            [Range(
                2,
                1000,
                ErrorMessage = "A lotação deve estar entre 2 e 1000.")]
            [Display(Name = "Número máximo de participantes")]
            public int NumMaxParticipantes { get; set; } = 10;

            [Display(Name = "Evento privado")]
            public bool IsPrivado { get; set; }

            [Display(Name = "Latitude")]
            public double? Latitude { get; set; }

            [Display(Name = "Longitude")]
            public double? Longitude { get; set; }

            [MinLength(
                1,
                ErrorMessage = "Seleciona pelo menos uma categoria.")]
            [Display(Name = "Categorias")]
            public List<int> CategoriasSelecionadas { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            await CarregarCategoriasAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.DataHora <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "Input.DataHora",
                    "A data do evento deve ser futura.");
            }

            List<int> categoriasSelecionadas = Input
                .CategoriasSelecionadas
                .Distinct()
                .ToList();

            List<int> categoriasValidas = await _context.Categorias
                .Where(c => categoriasSelecionadas.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (categoriasValidas.Count != categoriasSelecionadas.Count)
            {
                ModelState.AddModelError(
                    "Input.CategoriasSelecionadas",
                    "Uma das categorias selecionadas não é válida.");
            }

            if (!ModelState.IsValid)
            {
                await CarregarCategoriasAsync();
                return Page();
            }

            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = new Evento
            {
                Titulo = Input.Titulo.Trim(),
                Descricao = Input.Descricao.Trim(),
                DataHora = Input.DataHora,
                NumMaxParticipantes = Input.NumMaxParticipantes,
                IsPrivado = Input.IsPrivado,
                Latitude = Input.Latitude,
                Longitude = Input.Longitude,
                Estado = EstadoEvento.ParaBreve,
                CriadorId = utilizadorId
            };

            foreach (int categoriaId in categoriasValidas)
            {
                evento.EventosCategorias.Add(new EventoCategoria
                {
                    CategoriaId = categoriaId
                });
            }

            // O criador entra automaticamente como participante do evento.
            evento.Participantes.Add(new Participante
            {
                UtilizadorId = utilizadorId,
                Estado = EstadoPedido.Aceite,
                DataPedido = DateTime.Now
            });

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "O evento foi criado com sucesso.";

            return RedirectToPage("./Index");
        }

        private async Task CarregarCategoriasAsync()
        {
            Categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nome
                })
                .ToListAsync();
        }
    }
}
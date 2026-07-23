using JoinIt.Web.Data;
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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(
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
            public int Id { get; set; }

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

            [Range(
                2,
                1000,
                ErrorMessage = "A lotação deve estar entre 2 e 1000.")]
            [Display(Name = "Número máximo de participantes")]
            public int NumMaxParticipantes { get; set; }

            [Display(Name = "Evento privado")]
            public bool IsPrivado { get; set; }

            [Display(Name = "Latitude")]
            public double? Latitude { get; set; }

            [Display(Name = "Longitude")]
            public double? Longitude { get; set; }

            [Display(Name = "Categorias")]
            public List<int> CategoriasSelecionadas { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.EventosCategorias)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento is null)
            {
                return NotFound();
            }

            // Apenas o criador pode editar o evento.
            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            Input = new EventoInputModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Descricao = evento.Descricao,
                DataHora = evento.DataHora,
                NumMaxParticipantes = evento.NumMaxParticipantes,
                IsPrivado = evento.IsPrivado,
                Latitude = evento.Latitude,
                Longitude = evento.Longitude,
                CategoriasSelecionadas = evento.EventosCategorias
                    .Select(ec => ec.CategoriaId)
                    .ToList()
            };

            await CarregarCategoriasAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string? utilizadorId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(utilizadorId))
            {
                return Challenge();
            }

            var evento = await _context.Eventos
                .Include(e => e.EventosCategorias)
                .FirstOrDefaultAsync(e => e.Id == Input.Id);

            if (evento is null)
            {
                return NotFound();
            }

            // A verificação também é feita no POST por segurança.
            if (evento.CriadorId != utilizadorId)
            {
                return Forbid();
            }

            if (Input.DataHora <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "Input.DataHora",
                    "A data do evento deve ser futura.");
            }

            var categoriasSelecionadas = Input.CategoriasSelecionadas
                .Distinct()
                .ToList();

            if (categoriasSelecionadas.Count == 0)
            {
                ModelState.AddModelError(
                    "Input.CategoriasSelecionadas",
                    "Seleciona pelo menos uma categoria.");
            }

            var categoriasValidas = await _context.Categorias
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

            evento.Titulo = Input.Titulo.Trim();
            evento.Descricao = Input.Descricao.Trim();
            evento.DataHora = Input.DataHora;
            evento.NumMaxParticipantes = Input.NumMaxParticipantes;
            evento.IsPrivado = Input.IsPrivado;
            evento.Latitude = Input.Latitude;
            evento.Longitude = Input.Longitude;

            AtualizarCategorias(evento, categoriasValidas);

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                "O evento foi atualizado com sucesso.";

            return RedirectToPage(
                "./Details",
                new { id = evento.Id });
        }

        private void AtualizarCategorias(
            Evento evento,
            List<int> categoriasSelecionadas)
        {
            var categoriasAtuais = evento.EventosCategorias
                .Select(ec => ec.CategoriaId)
                .ToList();

            var categoriasParaRemover = evento.EventosCategorias
                .Where(ec =>
                    !categoriasSelecionadas.Contains(ec.CategoriaId))
                .ToList();

            _context.EventosCategorias.RemoveRange(
                categoriasParaRemover);

            var categoriasParaAdicionar = categoriasSelecionadas
                .Where(id => !categoriasAtuais.Contains(id));

            foreach (int categoriaId in categoriasParaAdicionar)
            {
                evento.EventosCategorias.Add(
                    new EventoCategoria
                    {
                        EventoId = evento.Id,
                        CategoriaId = categoriaId
                    });
            }
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
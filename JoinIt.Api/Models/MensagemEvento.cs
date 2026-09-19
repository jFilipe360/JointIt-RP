using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    // Representa uma mensagem enviada no chat de um evento
    public class MensagemEvento
    {
        public int Id { get; set; }

        // Evento ao qual a mensagem pertence
        public int EventoId { get; set; }

        [ValidateNever]
        public Evento Evento { get; set; } = null!;

        // Utilizador que enviou a mensagem
        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        [ValidateNever]
        public ApplicationUser Utilizador { get; set; } = null!;

        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(500, ErrorMessage = "A mensagem não pode ultrapassar 500 caracteres.")]
        public string Conteudo { get; set; } = string.Empty;

        [Display(Name = "Enviada em")]
        public DateTime EnviadaEm { get; set; } = DateTime.Now;
    }
}


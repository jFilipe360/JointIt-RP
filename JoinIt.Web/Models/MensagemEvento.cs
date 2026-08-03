using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class MensagemEvento
    {
        public int Id { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ValidateNever]
        public Evento Evento { get; set; } = null!;

        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        [ValidateNever]
        public ApplicationUser Utilizador { get; set; } = null!;

        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(
            500,
            ErrorMessage = "A mensagem não pode ultrapassar 500 caracteres.")]
        public string Conteudo { get; set; } = string.Empty;

        [Display(Name = "Enviada em")]
        public DateTime EnviadaEm { get; set; } = DateTime.Now;
    }
}
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    //Representa uma mensagem enviada no chat de um evento
    public class MensagemEvento
    {
        public int Id { get; set; }

        //Evento ao qual a mensagem pertence
        [Required]
        public int EventoId { get; set; }

        //As propriedades de navegação não são validadas no model binding
        [ValidateNever]
        public Evento Evento { get; set; } = null!;

        //Utilizador que enviou a mensagem
        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        [ValidateNever]
        public ApplicationUser Utilizador { get; set; } = null!;

        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(500, ErrorMessage = "A mensagem não pode ultrapassar 500 caracteres.")]
        [Display(Name = "Mensagem")]
        public string Conteudo { get; set; } = string.Empty;

        [Display(Name = "Enviada em")]
        public DateTime EnviadaEm { get; set; } = DateTime.Now;
    }
}
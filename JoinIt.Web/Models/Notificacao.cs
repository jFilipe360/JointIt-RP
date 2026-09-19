using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    //Representa uma notificação enviada a um utilizador
    public class Notificacao
    {
        public int Id { get; set; }

        //Utilizador a quem a notificação é enviada
        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        public ApplicationUser Utilizador { get; set; } = null!;

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título não pode ultrapassar 100 caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(500, ErrorMessage = "A mensagem não pode ultrapassar 500 caracteres.")]
        [Display(Name = "Mensagem")]
        public string Mensagem { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "O link não pode ultrapassar 500 caracteres.")]
        [Display(Name = "Link")]
        public string? Link { get; set; }

        [Display(Name = "Lida")]
        public bool Lida { get; set; }

        [Display(Name = "Criado em")]
        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    public class Notificacao
    {
        public int Id { get; set; }

        [Required]
        public string UtilizadorId { get; set; } = string.Empty;

        public ApplicationUser Utilizador { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Mensagem { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Link { get; set; }

        public bool Lida { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}


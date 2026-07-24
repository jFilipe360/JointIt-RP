using JoinIt.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class Amizade
    {
        public int Id { get; set; }

        [Required]
        public string EmissorId { get; set; } = string.Empty;

        public ApplicationUser Emissor { get; set; } = null!;

        [Required]
        public string RecetorId { get; set; } = string.Empty;

        public ApplicationUser Recetor { get; set; } = null!;

        public EstadoPedido Estado { get; set; }
            = EstadoPedido.Pendente;

        public DateTime CriadoEm { get; set; }
            = DateTime.Now;
    }
}
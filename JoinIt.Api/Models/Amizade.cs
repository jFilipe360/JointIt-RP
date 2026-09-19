using JoinIt.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    // Representa uma relação de amizade entre dois utilizadores
    public class Amizade
    {
        public int Id { get; set; }

        // Utilizador que enviou o pedido de amizade
        [Required(ErrorMessage = "O emissor é obrigatório.")]
        public string EmissorId { get; set; } = string.Empty;

        public ApplicationUser Emissor { get; set; } = null!;

        // Utilizador que recebeu o pedido de amizade
        [Required(ErrorMessage = "O recetor é obrigatório.")]
        public string RecetorId { get; set; } = string.Empty;

        public ApplicationUser Recetor { get; set; } = null!;

        public EstadoPedido Estado { get; set; }
            = EstadoPedido.Pendente;

        public DateTime CriadoEm { get; set; }
            = DateTime.Now;
    }
}


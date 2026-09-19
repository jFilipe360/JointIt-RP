using JoinIt.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    // Representa um convite enviado a um utilizador para participar num evento
    public class ConviteEvento
    {
        public int Id { get; set; }

        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        // Utilizador que enviou o convite
        [Required]
        public string EmissorId { get; set; } = string.Empty;

        public ApplicationUser Emissor { get; set; } = null!;

        // Utilizador que recebeu o convite
        [Required]
        public string RecetorId { get; set; } = string.Empty;

        public ApplicationUser Recetor { get; set; } = null!;

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? RespondidoEm { get; set; }
    }
}


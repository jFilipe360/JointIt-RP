using JoinIt.Api.Enums;

namespace JoinIt.Api.Models
{
    // Representa a participação de um utilizador num evento
    public class Participante
    {
        // Evento associado à participação
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        // Utilizador associado à participação
        public string UtilizadorId { get; set; } = string.Empty;

        public ApplicationUser Utilizador { get; set; } = null!;

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        public DateTime DataPedido { get; set; } = DateTime.Now;
    }
}


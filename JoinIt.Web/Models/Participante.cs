using JoinIt.Web.Enums;

namespace JoinIt.Web.Models
{
    public class Participante
    {
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        public string UtilizadorId { get; set; } = string.Empty;

        public ApplicationUser Utilizador { get; set; } = null!;

        public EstadoPedido Estado { get; set; }
            = EstadoPedido.Pendente;

        public DateTime DataPedido { get; set; }
            = DateTime.Now;
    }
}
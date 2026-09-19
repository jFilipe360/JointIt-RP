using JoinIt.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    //Representa a participação de um utilizador num evento
    public class Participante
    {
        //Evento associado à participação
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        //Utilizador associado à participação
        public string UtilizadorId { get; set; } = string.Empty;

        public ApplicationUser Utilizador { get; set; } = null!;

        //Estado atual da participação (Pendente, Aceite, Rejeitada)
        [Display(Name = "Estado do Pedido")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        //Data em que o pedido de participação foi feito
        [Display(Name = "Data do Pedido")]
        public DateTime DataPedido { get; set; } = DateTime.Now;
    }
}
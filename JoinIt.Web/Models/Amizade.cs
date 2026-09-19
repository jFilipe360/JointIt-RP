using JoinIt.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class Amizade
    {
        public int Id { get; set; }

        //Utilizador que enviou o pedido de amizade.
        [Required]
        public string EmissorId { get; set; } = string.Empty;

        public ApplicationUser Emissor { get; set; } = null!;

        //Utilizador que recebeu o pedido de amizade.
        [Required]
        public string RecetorId { get; set; } = string.Empty;

        public ApplicationUser Recetor { get; set; } = null!;

        //Estado do pedido de amizade (Pendente, Aceite, Recusado).
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        //Data em que o pedido de amizade foi criado.
        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
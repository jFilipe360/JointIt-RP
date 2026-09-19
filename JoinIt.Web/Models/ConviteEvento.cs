using JoinIt.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class ConviteEvento
    {
        public int Id { get; set; }

        //Evento associado ao convite
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        //Utilizador que enviou o convite
        [Required]
        [Display(Name = "Emissor")]
        public string EmissorId { get; set; } = string.Empty;

        public ApplicationUser Emissor { get; set; } = null!;

        //Utilizador que recebeu o convite
        [Required]
        [Display(Name = "Recetor")]
        public string RecetorId { get; set; } = string.Empty;

        public ApplicationUser Recetor { get; set; } = null!;

        //Estado do convite (Pendente, Aceite, Recusado)
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        [Display(Name = "Criado em")]
        public DateTime CriadoEm { get; set; } = DateTime.Now;

        //Data em que o convite foi respondido (aceite ou recusado)
        [Display(Name = "Respondido em")]
        public DateTime? RespondidoEm { get; set; }
    }
}
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(300)]
        public string? FotoPerfil { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public ICollection<Evento> EventosCriados { get; set; } = new List<Evento>();

        public ICollection<Participante> Participacoes { get; set; } = new List<Participante>();

        public ICollection<Amizade> PedidosAmizadeEnviados { get; set; } = new List<Amizade>();

        public ICollection<Amizade> PedidosAmizadeRecebidos { get; set; } = new List<Amizade>();

        public ICollection<ConviteEvento> ConvitesEnviados { get; set; } = new List<ConviteEvento>();

        public ICollection<ConviteEvento> ConvitesRecebidos { get; set; } = new List<ConviteEvento>();

        public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();

        public ICollection<MensagemEvento> MensagensEnviadas { get; set; } = new List<MensagemEvento>();
    }
}
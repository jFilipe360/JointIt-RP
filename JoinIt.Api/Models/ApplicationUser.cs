using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    // Utilizador da aplicação, baseado no ASP.NET Core Identity
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(300)]
        public string? FotoPerfil { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        // Relações associadas ao utilizador
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


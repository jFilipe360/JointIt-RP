using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    //Utilizador da aplicação, baseado na classe IdentityUser do ASP.NET Core Identity
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "O URL da foto de perfil não pode ter mais de 300 caracteres.")]
        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfil { get; set; }

        [Display(Name = "Data de Criação")]
        public DateTime CriadoEm { get; set; } = DateTime.Now;

        //Relações com eventos
        public ICollection<Evento> EventosCriados { get; set; } = new List<Evento>();

        public ICollection<Participante> Participacoes { get; set; } = new List<Participante>();

        //Relações com amizades
        public ICollection<Amizade> PedidosAmizadeEnviados { get; set; } = new List<Amizade>();

        public ICollection<Amizade> PedidosAmizadeRecebidos { get; set; } = new List<Amizade>();

        //Convites para eventos
        public ICollection<ConviteEvento> ConvitesEnviados { get; set; } = new List<ConviteEvento>();

        public ICollection<ConviteEvento> ConvitesRecebidos { get; set; } = new List<ConviteEvento>();

        //Conteúdos relacionados com o utilizador
        public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();

        public ICollection<MensagemEvento> MensagensEnviadas { get; set; } = new List<MensagemEvento>();
    }
}
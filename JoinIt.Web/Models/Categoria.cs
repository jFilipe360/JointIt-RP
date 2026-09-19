using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome não pode ultrapassar 50 caracteres.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        //Relação muitos para muitos entre Categoria e Evento através da tabela de junção EventoCategoria
        public ICollection<EventoCategoria> EventosCategorias { get; set; } = new List<EventoCategoria>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Web.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(
            50,
            ErrorMessage = "O nome não pode ultrapassar 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public ICollection<EventoCategoria> EventosCategorias { get; set; }
            = new List<EventoCategoria>();
    }
}
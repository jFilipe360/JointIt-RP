using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.Models
{
    // Representa uma categoria associável a eventos
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome não pode ultrapassar 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        // Relações entre esta categoria e os eventos associados
        public ICollection<EventoCategoria> EventosCategorias { get; set; } = new List<EventoCategoria>();
    }
}


using System.Diagnostics;

namespace JoinIt.Api.Models
{
    public class EventoCategoria
    {
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;
    }
}


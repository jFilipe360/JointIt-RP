namespace JoinIt.Api.Models
{
    // Entidade de associação entre eventos e categorias
    public class EventoCategoria
    {
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;
    }
}


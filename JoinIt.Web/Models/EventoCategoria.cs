namespace JoinIt.Web.Models
{
    //Entidade de ligação entre Evento e Categoria (muitos para muitos)
    public class EventoCategoria
    {
        //Evento associado à relação
        public int EventoId { get; set; }

        public Evento Evento { get; set; } = null!;

        //Categoria associada à relação
        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; } = null!;
    }
}
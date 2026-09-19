namespace JoinIt.Api.DTOs.Eventos;

// Estrutura genérica usada para devolver resultados paginados pela API.
public class ResultadoPaginadoDto<T>
{
    public List<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}
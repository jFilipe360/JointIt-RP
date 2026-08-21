using JoinIt.Api.DTOs.Categorias;
using JoinIt.Api.Enums;

namespace JoinIt.Api.DTOs.Eventos;

public class EventoDetalhesDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public DateTime DataHora { get; set; }

    public DateTime DataFim { get; set; }

    public string Local { get; set; } = string.Empty;

    public string? Morada { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsPrivado { get; set; }

    public int NumMaxParticipantes { get; set; }

    public int NumParticipantes { get; set; }

    public int VagasDisponiveis { get; set; }

    public EstadoEvento Estado { get; set; }

    public string CriadorId { get; set; } = string.Empty;

    public string CriadorNome { get; set; } = string.Empty;

    public List<CategoriaDto> Categorias { get; set; } = [];
}
using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.DTOs.Eventos;

public class EditarEventoDto
{
    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    [Required]
    [StringLength(150)]
    public string Local { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Morada { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    public bool IsPrivado { get; set; }

    [Range(2, 1000)]
    public int NumMaxParticipantes { get; set; }

    public List<int> CategoriaIds { get; set; } = [];
}
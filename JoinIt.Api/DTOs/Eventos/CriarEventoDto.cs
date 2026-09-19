using System.ComponentModel.DataAnnotations;

namespace JoinIt.Api.DTOs.Eventos;

// Dados necessários para criar um novo evento através da API
public class CriarEventoDto
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título não pode ultrapassar 100 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(1000, ErrorMessage = "A descrição não pode ultrapassar 1000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    public DateTime DataHora { get; set; }

    public DateTime DataFim { get; set; }

    public bool IsOnline { get; set; }

    [StringLength(500, ErrorMessage = "O link do evento não pode ultrapassar 500 caracteres.")]
    public string? LinkOnline { get; set; }

    [StringLength(150, ErrorMessage = "O local não pode ultrapassar 150 caracteres.")]
    public string? Local { get; set; }

    [StringLength(250, ErrorMessage = "A morada não pode ultrapassar 250 caracteres.")]
    public string? Morada { get; set; }

    [Range(-90, 90, ErrorMessage = "A latitude deve estar entre -90 e 90.")]
    public double? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "A longitude deve estar entre -180 e 180.")]
    public double? Longitude { get; set; }

    public bool IsPrivado { get; set; }

    [Range(2, 1000, ErrorMessage = "O número máximo de participantes deve estar entre 2 e 1000.")]
    public int NumMaxParticipantes { get; set; }

    public List<int> CategoriaIds { get; set; } = [];
}
using JoinIt.Api.Enums;

namespace JoinIt.Api.DTOs.Convites;

public class ConviteDto
{
    public int Id { get; set; }

    public int EventoId { get; set; }

    public string EventoTitulo { get; set; } = string.Empty;

    public DateTime EventoDataHora { get; set; }

    public string EmissorId { get; set; } = string.Empty;

    public string EmissorNome { get; set; } = string.Empty;

    public EstadoPedido Estado { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime? RespondidoEm { get; set; }
}
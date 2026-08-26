using JoinIt.Api.Enums;

namespace JoinIt.Api.DTOs.Amigos;

public class PedidoAmizadeDto
{
    public int Id { get; set; }

    public string UtilizadorId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? FotoPerfil { get; set; }

    public EstadoPedido Estado { get; set; }

    public DateTime CriadoEm { get; set; }
}
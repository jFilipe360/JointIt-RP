namespace JoinIt.Api.DTOs.Amigos;

// Dados resumidos de um amigo devolvidos pela API
public class AmigoDto
{
    public int AmizadeId { get; set; }

    public string UtilizadorId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? FotoPerfil { get; set; }
}
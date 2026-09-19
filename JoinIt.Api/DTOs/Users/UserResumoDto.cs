namespace JoinIt.Api.DTOs.Users;

// Dados resumidos de um utilizador devolvidos pela API
public class UserResumoDto
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? FotoPerfil { get; set; }
}
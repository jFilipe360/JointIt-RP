using JoinIt.Api.DTOs.Eventos;

namespace JoinIt.Api.DTOs.Users;

// Dados detalhados de um utilizador, incluindo os seus eventos públicos
public class UserDetalhesDto
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? FotoPerfil { get; set; }

    public List<EventoResumoDto> EventosPublicos { get; set; } = [];
}
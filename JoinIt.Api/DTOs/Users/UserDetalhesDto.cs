using JoinIt.Api.DTOs.Eventos;

namespace JoinIt.Api.DTOs.Users;

public class UserDetalhesDto
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? FotoPerfil { get; set; }

    public List<EventoResumoDto> EventosPublicos { get; set; } = [];
}
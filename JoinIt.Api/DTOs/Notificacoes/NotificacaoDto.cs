namespace JoinIt.Api.DTOs.Notificacoes;

public class NotificacaoDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public string? Link { get; set; }

    public bool Lida { get; set; }

    public DateTime CriadoEm { get; set; }
}
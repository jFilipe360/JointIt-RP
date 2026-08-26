namespace JoinIt.Api.DTOs.Notificacoes;

public class NotificacoesResponseDto
{
    public int NumeroNaoLidas { get; set; }

    public List<NotificacaoDto> Notificacoes { get; set; } = [];
}
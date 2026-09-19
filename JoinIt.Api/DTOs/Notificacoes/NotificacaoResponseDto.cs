namespace JoinIt.Api.DTOs.Notificacoes;

// Resposta da API com as notificações e o número de notificações não lidas
public class NotificacoesResponseDto
{
    public int NumeroNaoLidas { get; set; }

    public List<NotificacaoDto> Notificacoes { get; set; } = [];
}
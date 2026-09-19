namespace JoinIt.Api.Services;

// Define as operações disponíveis para criação de notificações
public interface INotificacaoService
{
    Task CriarAsync(string utilizadorId, string titulo, string mensagem, string? link = null);
}
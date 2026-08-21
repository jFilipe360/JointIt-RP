namespace JoinIt.Api.Services;

public interface INotificacaoService
{
    Task CriarAsync(
        string utilizadorId,
        string titulo,
        string mensagem,
        string? link = null);
}
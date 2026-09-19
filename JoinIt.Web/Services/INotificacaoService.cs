namespace JoinIt.Web.Services
{
    // Define a operação usada para criar notificações para os utilizadores
    public interface INotificacaoService
    {
        Task CriarAsync(string utilizadorId, string titulo, string mensagem, string? link = null);
    }
}
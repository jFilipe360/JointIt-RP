using JoinIt.Web.Data;
using JoinIt.Web.Hubs;
using JoinIt.Web.Models;
using Microsoft.AspNetCore.SignalR;

namespace JoinIt.Web.Services
{
    // Cria notificações na base de dados e envia-as em tempo real através do SignalR
    public class NotificacaoService : INotificacaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacoesHub> _hubContext;

        public NotificacaoService(ApplicationDbContext context, IHubContext<NotificacoesHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CriarAsync(string utilizadorId, string titulo, string mensagem, string? link = null)
        {
            // Garante que os dados obrigatórios da notificação foram fornecidos
            if (string.IsNullOrWhiteSpace(utilizadorId))
            {
                throw new ArgumentException(
                    "O utilizador é obrigatório.",
                    nameof(utilizadorId));
            }

            if (string.IsNullOrWhiteSpace(titulo))
            {
                throw new ArgumentException(
                    "O título é obrigatório.",
                    nameof(titulo));
            }

            if (string.IsNullOrWhiteSpace(mensagem))
            {
                throw new ArgumentException(
                    "A mensagem é obrigatória.",
                    nameof(mensagem));
            }

            // Normaliza os dados antes de guardar a notificação
            var notificacao = new Notificacao
            {
                UtilizadorId = utilizadorId,
                Titulo = titulo.Trim(),
                Mensagem = mensagem.Trim(),
                Link = string.IsNullOrWhiteSpace(link)
                    ? null
                    : link.Trim(),
                Lida = false,
                CriadoEm = DateTime.Now
            };

            // Guarda primeiro a notificação para obter o respetivo identificador
            _context.Notificacoes.Add(notificacao);

            await _context.SaveChangesAsync();

            // Envia a notificação em tempo real apenas ao utilizador destinatário
            await _hubContext.Clients
                .User(utilizadorId)
                .SendAsync("ReceberNotificacao",
                    new
                    {
                        id = notificacao.Id,
                        titulo = notificacao.Titulo,
                        mensagem = notificacao.Mensagem,
                        link = notificacao.Link,
                        criadoEm = notificacao.CriadoEm
                    });
        }
    }
}
using JoinIt.Web.Data;
using JoinIt.Web.Models;

namespace JoinIt.Web.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly ApplicationDbContext _context;

        public NotificacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CriarAsync(
            string utilizadorId,
            string titulo,
            string mensagem,
            string? link = null)
        {
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

            _context.Notificacoes.Add(notificacao);

            await _context.SaveChangesAsync();
        }
    }
}
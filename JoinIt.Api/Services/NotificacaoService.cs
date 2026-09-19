using JoinIt.Api.Data;
using JoinIt.Api.Models;

namespace JoinIt.Api.Services;

// Cria e guarda notificações destinadas aos utilizadores
public class NotificacaoService : INotificacaoService
{
    private readonly ApplicationDbContext _context;

    public NotificacaoService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Valida e normaliza os dados antes de guardar a notificação
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

        titulo = titulo.Trim();
        mensagem = mensagem.Trim();
        link = string.IsNullOrWhiteSpace(link)
            ? null
            : link.Trim();

        if (titulo.Length > 100)
        {
            throw new ArgumentException(
                "O título não pode ultrapassar 100 caracteres.",
                nameof(titulo));
        }

        if (mensagem.Length > 500)
        {
            throw new ArgumentException(
                "A mensagem não pode ultrapassar 500 caracteres.",
                nameof(mensagem));
        }

        if (link?.Length > 500)
        {
            throw new ArgumentException(
                "O link não pode ultrapassar 500 caracteres.",
                nameof(link));
        }

        var notificacao = new Notificacao
        {
            UtilizadorId = utilizadorId,
            Titulo = titulo,
            Mensagem = mensagem,
            Link = link,
            Lida = false,
            CriadoEm = DateTime.Now
        };

        _context.Notificacoes.Add(notificacao);

        await _context.SaveChangesAsync();
    }
}
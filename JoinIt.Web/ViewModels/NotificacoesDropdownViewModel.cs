using JoinIt.Web.Models;

namespace JoinIt.Web.ViewModels
{
    // Dados necessários para apresentar o dropdown de notificações
    public class NotificacoesDropdownViewModel
    {
        public int NumeroNaoLidas { get; set; }

        public IList<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    }
}
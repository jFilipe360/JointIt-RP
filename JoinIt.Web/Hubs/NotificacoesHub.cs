using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JoinIt.Web.Hubs
{
    //Hub SignalR para enviar notificações em tempo real aos utilizadores autenticados.
    [Authorize]
    public class NotificacoesHub : Hub
    {
    }
}
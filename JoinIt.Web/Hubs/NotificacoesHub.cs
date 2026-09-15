using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JoinIt.Web.Hubs
{
    [Authorize]
    public class NotificacoesHub : Hub
    {
    }
}
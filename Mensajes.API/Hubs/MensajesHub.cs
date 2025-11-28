using Microsoft.AspNetCore.SignalR;

namespace Mensajes.API.Hubs
{
    public class MensajesHub : Hub
    {
        public async Task UnirseAConversacion(string conversacionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversacionId);
        }

        public async Task DejarConversacion(string conversacionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversacionId);
        }
    }
}

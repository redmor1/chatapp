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

        public async Task Escribiendo(string conversacionId, string usuarioNombre)
        {
            // Enviar a todos en el grupo EXCEPTO al que está escribiendo
            await Clients.GroupExcept(conversacionId, Context.ConnectionId).SendAsync("UsuarioEscribiendo", new { ConversacionId = conversacionId, Usuario = usuarioNombre });
        }

        public async Task DejoDeEscribir(string conversacionId, string usuarioNombre)
        {
            await Clients.GroupExcept(conversacionId, Context.ConnectionId).SendAsync("UsuarioDejoDeEscribir", new { ConversacionId = conversacionId, Usuario = usuarioNombre });
        }
    }
}

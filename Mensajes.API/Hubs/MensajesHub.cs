using Microsoft.AspNetCore.SignalR;

namespace Mensajes.API.Hubs
{
    public class MensajesHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var usuarioId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(usuarioId))
            {
                // Unir al usuario a un grupo con su propio ID para notificaciones personales
                await Groups.AddToGroupAsync(Context.ConnectionId, usuarioId);
            }
            await base.OnConnectedAsync();
        }
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

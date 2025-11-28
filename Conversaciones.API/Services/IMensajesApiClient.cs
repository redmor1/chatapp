namespace Conversaciones.API.Services
{
    public interface IMensajesApiClient
    {
        Task NotificarCambioMembresiaAsync(string tipo, string conversacionId, string usuarioId, string? nombreGrupo);
    }
}

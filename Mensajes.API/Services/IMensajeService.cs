using Mensajes.API.DTOs;

namespace Mensajes.API.Services
{
    public interface IMensajeService
    {
        Task<MensajesPaginadosResponse> GetHistorialMensajesAsync(string conversacionId, string tipo, string usuarioActualId, string? before, int limit);
        Task<MensajeResponse> EnviarMensajeAsync(string conversacionId, string tipo, CrearMensajeRequest request, string usuarioActualId);
        Task<AcuseLecturaResponse> MarcarMensajeComoLeidoAsync(string mensajeId, string usuarioActualId, MarcarComoLeidoRequest request);
        Task MarcarConversacionComoLeidaAsync(string conversacionId, string usuarioActualId);
        Task<List<AcuseLecturaResponse>> GetAcusesDeLecturaAsync(string mensajeId, string usuarioActualId);
        Task<bool> EsMiembroDeConversacionAsync(string conversacionId, string tipo, string usuarioId);
    }
}

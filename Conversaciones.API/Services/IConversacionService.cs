using Conversaciones.API.DTOs;

namespace Conversaciones.API.Services
{
    public interface IConversacionService
    {
        // Listar todas las conversaciones del usuario
        Task<List<ConversacionResponse>> GetConversacionesParaUsuarioAsync(string usuarioId);
        
        // Obtener una conversación por ID
        Task<ConversacionResponse?> GetConversacionPorIdAsync(Guid conversacionId, string usuarioId);
        
        // Crear un grupo
        Task<ConversacionResponse> CrearGrupoAsync(CrearGrupoRequest request, string creadorId);
        
        // Iniciar o recuperar chat directo
        Task<(ConversacionResponse conversacion, bool esNueva)> IniciarChatDirectoAsync(string usuarioActualId, string emailOtroUsuario);
        
        // Actualizar grupo (nombre, avatar)
        Task<ConversacionResponse?> ActualizarGrupoAsync(Guid conversacionId, ActualizarGrupoRequest request, string usuarioId);
        
        // Eliminar/abandonar conversación
        Task<bool> EliminarConversacionAsync(Guid conversacionId, string usuarioId);
        
        // Agregar miembro a grupo
        Task<bool> AgregarMiembroAsync(Guid conversacionId, string emailUsuario, string usuarioQueAgrega);
        
        // Quitar miembro de conversación
        Task<bool> QuitarMiembroAsync(Guid conversacionId, string usuarioIdAQuitar, string usuarioQueQuita);
        
        // Listar miembros
        Task<List<UsuarioResumenResponse>> GetMiembrosAsync(Guid conversacionId, string usuarioId);
    }
}

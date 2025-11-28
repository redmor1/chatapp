using Conversaciones.API.Data;
using Conversaciones.API.DTOs;
using Conversaciones.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conversaciones.API.Services
{
    public class ConversacionService : IConversacionService
    {
        private readonly ConversacionesDbContext _context;
        private readonly ILogger<ConversacionService> _logger;
        private readonly IUsuariosApiClient _usuariosApiClient;


        public ConversacionService(ConversacionesDbContext context, ILogger<ConversacionService> logger, IUsuariosApiClient usuariosApiClient)
        {
            _context = context;
            _logger = logger;
            _usuariosApiClient = usuariosApiClient;
        }

        public async Task<List<ConversacionResponse>> GetConversacionesParaUsuarioAsync(string usuarioActualId)
        {
            _logger.LogInformation("Buscando conversaciones para el usuario {UsuarioId}", usuarioActualId);

            // Obtener entidades de la BD
            var conversacionesEntidad = await _context.Conversaciones
                .Where(c => c.MiembrosConversacion.Any(m => m.UsuarioId == usuarioActualId))
                .Include(c => c.MiembrosConversacion)
                .ToListAsync();

            // Obtener todos los IDs de usuarios únicos de todas las conversaciones
            var usuarioIds = conversacionesEntidad
                .SelectMany(c => c.MiembrosConversacion.Select(m => m.UsuarioId))
                .Distinct()
                .ToList();

            // Llamada al servicio de Usuarios para obtener información en batch
            var usuarios = await _usuariosApiClient.GetUsuariosBatchAsync(usuarioIds);
            var usuariosDict = usuarios.ToDictionary(u => u.Id, u => u);

            // Mapping a DTOs
            var conversacionesResponse = new List<ConversacionResponse>();

            foreach (var conversacion in conversacionesEntidad)
            {
                // Mapeo de la lista de miembros
                var miembrosResponse = new List<UsuarioResumenResponse>();
                foreach (var miembro in conversacion.MiembrosConversacion)
                {
                    if (usuariosDict.TryGetValue(miembro.UsuarioId, out var usuario))
                    {
                        miembrosResponse.Add(usuario);
                    }
                    else
                    {
                        // Fallback si el usuario no fue encontrado
                        miembrosResponse.Add(new UsuarioResumenResponse(
                            miembro.UsuarioId,
                            "Usuario Desconocido",
                            null
                        ));
                    }
                }

                // Mapeo de la conversación principal
                conversacionesResponse.Add(new ConversacionResponse(
                    conversacion.Id,
                    conversacion.Tipo,
                    conversacion.Nombre,
                    conversacion.AvatarUrl,
                    miembrosResponse
                ));
            }

            return conversacionesResponse;
        }

        public async Task<ConversacionResponse?> GetConversacionPorIdAsync(Guid conversacionId, string usuarioId)
        {
            var conversacion = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .Where(c => c.Id == conversacionId)
                .Where(c => c.MiembrosConversacion.Any(m => m.UsuarioId == usuarioId))
                .FirstOrDefaultAsync();

            if (conversacion == null)
                return null;

            return await MapearConversacionAResponse(conversacion);
        }

        public async Task<ConversacionResponse> CrearGrupoAsync(CrearGrupoRequest request, string usuarioActualId)
        {
            _logger.LogInformation("Usuario {CreadorId} está creando un grupo llamado {Nombre}", usuarioActualId, request.Nombre);

            // Validar miembros iniciales si los hay
            if (request.EmailsMiembros != null && request.EmailsMiembros.Any())
            {
                // Eliminar duplicados y el propio email del usuario si lo hubiera (aunque no tenemos el email del usuario actual aquí fácilmente, 
                // asumimos que el front no manda el propio email, o lo filtramos después al obtener IDs)
                var emailsAValidar = request.EmailsMiembros.Distinct().ToList();
                
                foreach (var email in emailsAValidar)
                {
                    var usuario = await _usuariosApiClient.GetUsuarioPorEmailAsync(email);
                    if (usuario != null)
                    {
                        // Evitar agregar al creador dos veces
                        if (usuario.Id != usuarioActualId)
                        {
                            listaMiembros.Add(new MiembrosConversacion
                            {
                                Id = Guid.NewGuid(),
                                ConversacionId = nuevoGrupo.Id,
                                UsuarioId = usuario.Id,
                                Rol = "miembro",
                            });
                        }
                    }
                    else
                    {
                         _logger.LogWarning("Usuario con email {Email} no encontrado al crear grupo", email);
                         // Opcional: Lanzar error si un email no existe
                         // throw new ArgumentException($"El usuario con email {email} no existe");
                    }
                }
            }

            // Crear entidad Conversacion tipo grupo
            var nuevoGrupo = new Conversacion
            {
                Id = Guid.NewGuid(),
                Tipo = "grupo",
                Nombre = request.Nombre,
                CreadorId = usuarioActualId,
                FechaCreacion = DateTime.UtcNow,
                AvatarUrl = null
            };

            // Preparar la lista de miembros
            var listaMiembros = new List<MiembrosConversacion>();

            // Agregar al CREADOR como el primer miembro (y admin)
            var miembroCreador = new MiembrosConversacion
            {
                Id = Guid.NewGuid(),
                ConversacionId = nuevoGrupo.Id,
                UsuarioId = usuarioActualId,
                Rol = "admin",
            };
            listaMiembros.Add(miembroCreador);

            // (Lógica de agregar miembros movida arriba para resolver emails primero)

            // Agregar todo a la BD en una transacción
            await _context.Conversaciones.AddAsync(nuevoGrupo);
            await _context.MiembrosConversacion.AddRangeAsync(listaMiembros);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Grupo {ConversacionId} creado exitosamente", nuevoGrupo.Id);

            // Obtener información de usuarios del servicio de Usuarios
            var usuarioIds = listaMiembros.Select(m => m.UsuarioId).ToList();
            var usuarios = await _usuariosApiClient.GetUsuariosBatchAsync(usuarioIds);
            var usuariosDict = usuarios.ToDictionary(u => u.Id, u => u);

            var miembrosResponse = listaMiembros.Select(m =>
            {
                if (usuariosDict.TryGetValue(m.UsuarioId, out var usuario))
                {
                    return usuario;
                }
                return new UsuarioResumenResponse(
                    m.UsuarioId,
                    "Usuario Desconocido",
                    null
                );
            }).ToList();

            return new ConversacionResponse(
                nuevoGrupo.Id,
                nuevoGrupo.Tipo,
                nuevoGrupo.Nombre,
                nuevoGrupo.AvatarUrl,
                miembrosResponse
            );
        }

        public async Task<(ConversacionResponse conversacion, bool esNueva)> IniciarChatDirectoAsync(string usuarioActualId, string emailOtroUsuario)
        {
            _logger.LogInformation("Usuario {UsuarioId} iniciando chat directo con email {Email}", usuarioActualId, emailOtroUsuario);

            // 1. Resolver Email a ID
            var otroUsuario = await _usuariosApiClient.GetUsuarioPorEmailAsync(emailOtroUsuario);
            if (otroUsuario == null)
            {
                 _logger.LogWarning("Intento de iniciar chat con email inexistente: {Email}", emailOtroUsuario);
                throw new ArgumentException($"El usuario con email {emailOtroUsuario} no existe");
            }
            
            var otroUsuarioId = otroUsuario.Id;

            if (otroUsuarioId == usuarioActualId)
            {
                throw new ArgumentException("No puedes iniciar un chat contigo mismo");
            }

            // 2. Buscar conversación directa existente entre los 2 usuarios
            var conversacionExistente = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .Where(c => c.Tipo == "directo")
                .Where(c => c.MiembrosConversacion.Count == 2)
                .Where(c => c.MiembrosConversacion.Any(m => m.UsuarioId == usuarioActualId))
                .Where(c => c.MiembrosConversacion.Any(m => m.UsuarioId == otroUsuarioId))
                .FirstOrDefaultAsync();

            if (conversacionExistente != null)
            {
                _logger.LogInformation("Conversación directa ya existe: {ConversacionId}", conversacionExistente.Id);
                var response = await MapearConversacionAResponse(conversacionExistente);
                return (response, false); // No es nueva
            }

            // 2. Crear nueva conversación directa
            var nuevaConversacion = new Conversacion
            {
                Id = Guid.NewGuid(),
                Tipo = "directo",
                Nombre = null,
                AvatarUrl = null,
                CreadorId = usuarioActualId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Conversaciones.Add(nuevaConversacion);

            // Agregar los 2 miembros
            _context.MiembrosConversacion.AddRange(
                new MiembrosConversacion
                {
                    Id = Guid.NewGuid(),
                    ConversacionId = nuevaConversacion.Id,
                    UsuarioId = usuarioActualId,
                    Rol = "miembro"
                },
                new MiembrosConversacion
                {
                    Id = Guid.NewGuid(),
                    ConversacionId = nuevaConversacion.Id,
                    UsuarioId = otroUsuarioId,
                    Rol = "miembro"
                }
            );

            await _context.SaveChangesAsync();

            _logger.LogInformation("Conversación directa creada: {ConversacionId}", nuevaConversacion.Id);
            var nuevaResponse = await MapearConversacionAResponse(nuevaConversacion);
            return (nuevaResponse, true); // Es nueva
        }

        public async Task<ConversacionResponse?> ActualizarGrupoAsync(Guid conversacionId, ActualizarGrupoRequest request, string usuarioId)
        {
            var conversacion = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .FirstOrDefaultAsync(c => c.Id == conversacionId);

            if (conversacion == null)
                return null;

            // Solo se pueden actualizar grupos
            if (conversacion.Tipo != "grupo")
                return null;

            // Verificar que el usuario sea admin
            var esAdmin = conversacion.MiembrosConversacion.Any(m => m.UsuarioId == usuarioId && m.Rol == "admin");
            if (!esAdmin)
                return null;

            // Actualizar campos
            if (!string.IsNullOrEmpty(request.Nombre))
                conversacion.Nombre = request.Nombre;

            if (request.AvatarUrl != null)
                conversacion.AvatarUrl = request.AvatarUrl;

            await _context.SaveChangesAsync();

            return await MapearConversacionAResponse(conversacion);
        }

        public async Task<bool> EliminarConversacionAsync(Guid conversacionId, string usuarioId)
        {
            var conversacion = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .FirstOrDefaultAsync(c => c.Id == conversacionId);

            if (conversacion == null)
                return false;

            if (conversacion.Tipo == "grupo")
            {
                // Solo el admin puede eliminar el grupo completo
                var esAdmin = conversacion.MiembrosConversacion.Any(m => m.UsuarioId == usuarioId && m.Rol == "admin");
                if (!esAdmin)
                    return false;

                _context.Conversaciones.Remove(conversacion);
            }
            else // directo
            {
                // El usuario se quita a sí mismo
                var miembro = conversacion.MiembrosConversacion.FirstOrDefault(m => m.UsuarioId == usuarioId);
                if (miembro != null)
                    _context.MiembrosConversacion.Remove(miembro);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AgregarMiembroAsync(Guid conversacionId, string emailUsuario, string usuarioQueAgrega)
        {
            var conversacion = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .FirstOrDefaultAsync(c => c.Id == conversacionId);

            if (conversacion == null || conversacion.Tipo != "grupo")
                return false;

            // Verificar que quien agrega sea admin
            var esAdmin = conversacion.MiembrosConversacion.Any(m => m.UsuarioId == usuarioQueAgrega && m.Rol == "admin");
            if (!esAdmin)
                return false;

            // Buscar usuario por email
            var usuario = await _usuariosApiClient.GetUsuarioPorEmailAsync(emailUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("Intento de agregar usuario inexistente por email: {Email}", emailUsuario);
                return false;
            }

            // Verificar que el usuario no sea ya miembro
            if (conversacion.MiembrosConversacion.Any(m => m.UsuarioId == usuario.Id))
                return false;

            _context.MiembrosConversacion.Add(new MiembrosConversacion
            {
                Id = Guid.NewGuid(),
                ConversacionId = conversacionId,
                UsuarioId = usuario.Id,
                Rol = "miembro"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuitarMiembroAsync(Guid conversacionId, string usuarioIdAQuitar, string usuarioQueQuita)
        {
            var conversacion = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .FirstOrDefaultAsync(c => c.Id == conversacionId);

            if (conversacion == null)
                return false;

            // Verificar permisos según tipo
            if (conversacion.Tipo == "grupo")
            {
                var esAdmin = conversacion.MiembrosConversacion.Any(m => m.UsuarioId == usuarioQueQuita && m.Rol == "admin");
                if (!esAdmin && usuarioQueQuita != usuarioIdAQuitar)
                    return false;
            }

            var miembro = conversacion.MiembrosConversacion.FirstOrDefault(m => m.UsuarioId == usuarioIdAQuitar);
            if (miembro == null)
                return false;

            _context.MiembrosConversacion.Remove(miembro);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UsuarioResumenResponse>> GetMiembrosAsync(Guid conversacionId, string usuarioId)
        {
            var conversacion = await _context.Conversaciones
                .Include(c => c.MiembrosConversacion)
                .Where(c => c.Id == conversacionId)
                .Where(c => c.MiembrosConversacion.Any(m => m.UsuarioId == usuarioId))
                .FirstOrDefaultAsync();

            if (conversacion == null)
                return new List<UsuarioResumenResponse>();

            var usuarioIds = conversacion.MiembrosConversacion.Select(m => m.UsuarioId).ToList();
            var usuarios = await _usuariosApiClient.GetUsuariosBatchAsync(usuarioIds);

            return usuarios;
        }

        // Método helper para mapear entidad a DTO
        private async Task<ConversacionResponse> MapearConversacionAResponse(Conversacion conversacion)
        {
            var usuarioIds = conversacion.MiembrosConversacion.Select(m => m.UsuarioId).ToList();
            var usuarios = await _usuariosApiClient.GetUsuariosBatchAsync(usuarioIds);
            var usuariosDict = usuarios.ToDictionary(u => u.Id, u => u);

            var miembrosResponse = conversacion.MiembrosConversacion.Select(m =>
            {
                if (usuariosDict.TryGetValue(m.UsuarioId, out var usuario))
                {
                    return usuario;
                }
                return new UsuarioResumenResponse(
                    m.UsuarioId,
                    "Usuario Desconocido",
                    null
                );
            }).ToList();

            return new ConversacionResponse(
                conversacion.Id,
                conversacion.Tipo,
                conversacion.Nombre,
                conversacion.AvatarUrl,
                miembrosResponse
            );
        }
    }
}

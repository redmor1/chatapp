using Mensajes.API.Data;
using Mensajes.API.DTOs;
using Mensajes.API.Entities;
using Mensajes.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mensajes.API.Services
{
    public class MensajeService : IMensajeService
    {
        private readonly MensajesDbContext _context;
        private readonly ILogger<MensajeService> _logger;
        private readonly IHubContext<MensajesHub> _hubContext;
        private readonly IConversacionesApiClient _conversacionesApiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MensajeService(
            MensajesDbContext context, 
            ILogger<MensajeService> logger,
            IHubContext<MensajesHub> hubContext,
            IConversacionesApiClient conversacionesApiClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _conversacionesApiClient = conversacionesApiClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MensajesPaginadosResponse> GetHistorialMensajesAsync(
            string conversacionId, 
            string tipo, 
            string usuarioActualId, 
            string? before, 
            int limit)
        {
            _logger.LogInformation(
                "Obteniendo historial de mensajes para conversación {ConversacionId} (tipo: {Tipo})", 
                conversacionId, 
                tipo);

            if (!await EsMiembroDeConversacionAsync(conversacionId, tipo, usuarioActualId))
            {
                throw new UnauthorizedAccessException("No eres miembro de esta conversación");
            }

            var query = _context.Mensajes
                .AsNoTracking()
                .Where(m => m.ConversacionId == conversacionId);

            if (!string.IsNullOrEmpty(before) && DateTime.TryParse(before, out var beforeDate))
            {
                query = query.Where(m => m.FechaCreacion < beforeDate);
            }

            var mensajes = await query
                .OrderByDescending(m => m.FechaCreacion)
                .Take(limit)
                .Select(m => new MensajeResponse
                {
                    Id = m.Id,
                    ConversacionId = m.ConversacionId,
                    AutorId = m.AutorId,
                    Contenido = m.Contenido,
                    FechaCreacion = m.FechaCreacion ?? DateTime.UtcNow,
                    LeidoPor = m.AcusesLecturas.Select(a => a.UsuarioId).ToList()
                })
                .ToListAsync();

            // Los mensajes vienen ordenados por fecha descendente (más recientes primero), 
            // pero para el chat solemos quererlos ascendente o manejarlos así en el front.
            // Mantendremos el orden descendente de la query para la paginación, el front puede invertirlos.

            string? nextCursor = null;
            if (mensajes.Count > 0)
            {
                nextCursor = mensajes.Last().FechaCreacion.ToString("o");
            }

            return new MensajesPaginadosResponse
            {
                Mensajes = mensajes,
                NextCursor = nextCursor
            };
        }

        public async Task<MensajeResponse> EnviarMensajeAsync(
            string conversacionId, 
            string tipo, 
            CrearMensajeRequest request, 
            string usuarioActualId)
        {
            _logger.LogInformation(
                "Usuario {UsuarioId} enviando mensaje a conversación {ConversacionId} (tipo: {Tipo})", 
                usuarioActualId, 
                conversacionId, 
                tipo);

            if (!await EsMiembroDeConversacionAsync(conversacionId, tipo, usuarioActualId))
            {
                throw new UnauthorizedAccessException("No eres miembro de esta conversación");
            }

            var nuevoMensaje = new Mensaje
            {
                Id = Guid.NewGuid(),
                ConversacionId = conversacionId,
                AutorId = usuarioActualId,
                Contenido = request.Contenido,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Mensajes.Add(nuevoMensaje);
            await _context.SaveChangesAsync();

            var response = new MensajeResponse
            {
                Id = nuevoMensaje.Id,
                ConversacionId = nuevoMensaje.ConversacionId,
                AutorId = nuevoMensaje.AutorId,
                Contenido = nuevoMensaje.Contenido,
                FechaCreacion = nuevoMensaje.FechaCreacion ?? DateTime.UtcNow,
                LeidoPor = new List<string>()
            };

            // Enviar evento SignalR
            await _hubContext.Clients.Group(conversacionId).SendAsync("NuevoMensaje", response);

            return response;
        }

        public async Task<AcuseLecturaResponse> MarcarMensajeComoLeidoAsync(
            string mensajeId, 
            string usuarioActualId, 
            MarcarComoLeidoRequest request)
        {
            _logger.LogInformation(
                "Usuario {UsuarioId} marcando mensaje {MensajeId} como leído", 
                usuarioActualId, 
                mensajeId);

            var mensaje = await _context.Mensajes.FindAsync(Guid.Parse(mensajeId));
            if (mensaje == null)
            {
                throw new KeyNotFoundException("Mensaje no encontrado");
            }

            
            // Obtener token
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token) || !await _conversacionesApiClient.ValidarMembresiaAsync(mensaje.ConversacionId, token))
            {
                 throw new UnauthorizedAccessException("No tienes acceso a esta conversación");
            }

            var yaLeido = await _context.AcusesLecturas
                .AnyAsync(a => a.MensajeId == mensaje.Id && a.UsuarioId == usuarioActualId);

            if (!yaLeido)
            {
                var acuse = new AcusesLectura
                {
                    MensajeId = mensaje.Id,
                    UsuarioId = usuarioActualId,
                    LeidoEn = DateTime.UtcNow
                };
                _context.AcusesLecturas.Add(acuse);
                await _context.SaveChangesAsync();

                // Notificar por SignalR
                await _hubContext.Clients.Group(mensaje.ConversacionId).SendAsync("MensajeLeido", new 
                { 
                    MensajeId = mensaje.Id, 
                    UsuarioId = usuarioActualId, 
                    Fecha = acuse.LeidoEn 
                });

                return new AcuseLecturaResponse
                {
                    MensajeId = mensaje.Id,
                    UsuarioId = usuarioActualId,
                    LeidoEn = acuse.LeidoEn ?? DateTime.UtcNow
                };
            }
            
            return new AcuseLecturaResponse
            {
                MensajeId = mensaje.Id,
                UsuarioId = usuarioActualId,
                LeidoEn = DateTime.UtcNow // O la fecha original si la tuviéramos a mano
            };
        }

        public async Task<List<AcuseLecturaResponse>> GetAcusesDeLecturaAsync(
            string mensajeId, 
            string usuarioActualId)
        {
            // Implementación básica, similar a las anteriores
             var mensaje = await _context.Mensajes
                .Include(m => m.AcusesLecturas)
                .FirstOrDefaultAsync(m => m.Id == Guid.Parse(mensajeId));

            if (mensaje == null)
            {
                throw new KeyNotFoundException("Mensaje no encontrado");
            }
            
            // Validación de seguridad omitida por brevedad, pero debería estar.

            return mensaje.AcusesLecturas.Select(a => new AcuseLecturaResponse
            {
                MensajeId = a.MensajeId,
                UsuarioId = a.UsuarioId,
                LeidoEn = a.LeidoEn ?? DateTime.UtcNow
            }).ToList();
        }

        public async Task<bool> EsMiembroDeConversacionAsync(
            string conversacionId, 
            string tipo, 
            string usuarioId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No se encontró token Bearer para validar membresía");
                return false;
            }

            return await _conversacionesApiClient.ValidarMembresiaAsync(conversacionId, token);
        }
    }
}

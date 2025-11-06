using Conversaciones.API.Data;
using Conversaciones.API.DTOs;
using Conversaciones.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conversaciones.API.Services
{
    public class GrupoService: IGrupoService
    {
        private readonly GruposDbContext _context;
        private readonly ILogger<GrupoService> _logger;
        private readonly IUsuariosApiClient _usuariosApiClient;


        public GrupoService(GruposDbContext context, ILogger<GrupoService> logger, IUsuariosApiClient usuariosApiClient)
        {
            _context = context;
            _logger = logger;
            _usuariosApiClient = usuariosApiClient;
        }

        public async Task<List<GrupoDetalleResponse>> GetGruposParaUsuarioAsync(string usuarioActualId) {

            _logger.LogInformation("Buscando grupos para el usuario {UsuarioId}", usuarioActualId);

            // Obtener entidades de la BD
            var gruposEntidad = await _context.Grupos
                .Where(g => g.MiembrosGrupos.Any(m => m.UsuarioId == usuarioActualId))
                .Include(g => g.MiembrosGrupos)
                .ToListAsync();

            // Obtener todos los IDs de usuarios únicos de todos los grupos
            var usuarioIds = gruposEntidad
                .SelectMany(g => g.MiembrosGrupos.Select(m => m.UsuarioId))
                .Distinct()
                .ToList();

            // Llamada al servicio de Usuarios para obtener información en batch
            var usuarios = await _usuariosApiClient.GetUsuariosBatchAsync(usuarioIds);
            var usuariosDict = usuarios.ToDictionary(u => u.Id, u => u);

            // Mapping a DTOs
            var gruposResponse = new List<GrupoDetalleResponse>();

            foreach (var grupo in gruposEntidad)
            {
                // Mapeo de la lista de miembros
                var miembrosResponse = new List<UsuarioResumenResponse>();
                foreach (var miembro in grupo.MiembrosGrupos)
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

                // Mapeo del grupo principal
                gruposResponse.Add(new GrupoDetalleResponse(
                    grupo.Id,
                    grupo.Nombre,
                    grupo.AvatarUrl,
                    miembrosResponse
                ));
            }

            return gruposResponse;
        }


        public async Task<ActionResult<GrupoDetalleResponse>> CrearGrupoAsync(CrearGrupoRequest request, string usuarioActualId)
        {
            _logger.LogInformation("Usuario {CreadorId} está creando un grupo llamado {Nombre}", usuarioActualId, request.Nombre);

            // Crear entidad Grupo
            var nuevoGrupo = new Grupo
            {
                Id = Guid.NewGuid(), // El scaffold de MySQL no puede hacer Guid automático
                Nombre = request.Nombre,
                CreadorId = usuarioActualId,
                FechaCreacion = DateTime.UtcNow
                // AvatarUrl se deja null por ahora
            };

            // Preparar la lista de miembros
            var listaMiembros = new List<MiembrosGrupo>();

            // Agregar al CREADOR como el primer miembro (y admin)
            var miembroCreador = new MiembrosGrupo
            {
                Id = Guid.NewGuid(),
                GrupoId = nuevoGrupo.Id,
                UsuarioId = usuarioActualId,
                Rol = "admin", // El creador es admin
            };
            listaMiembros.Add(miembroCreador);

            // Agregar a los otros miembros iniciales (si los hay)
            if (request.MiembrosIniciales != null && request.MiembrosIniciales.Any()) {             
                foreach (var miembroId in request.MiembrosIniciales)
            {
                // Evitar agregar al creador dos veces
                if (miembroId != usuarioActualId)
                {
                    listaMiembros.Add(new MiembrosGrupo
                    {
                        Id = Guid.NewGuid(),
                        GrupoId = nuevoGrupo.Id,
                        UsuarioId = miembroId,
                        Rol = "miembro", // Los miembros iniciales son 'miembro'
                    });
                }
            }}



            // Agregar todo a la BD en una transacción
            await _context.Grupos.AddAsync(nuevoGrupo);
            await _context.MiembrosGrupos.AddRangeAsync(listaMiembros);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Grupo {GrupoId} creado exitosamente", nuevoGrupo.Id);

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

            var responseDto = new GrupoDetalleResponse(
                nuevoGrupo.Id,
                nuevoGrupo.Nombre,
                nuevoGrupo.AvatarUrl,
                miembrosResponse
            );

            return responseDto;
        }
    }
}

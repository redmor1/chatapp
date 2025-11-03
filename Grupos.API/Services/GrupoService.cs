using Grupos.API.Data;
using Grupos.API.DTOs;
using Grupos.API.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grupos.API.Services
{
    public class GrupoService: IGrupoService
    {
        private readonly GruposDbContext _context;
        private readonly ILogger<GrupoService> _logger;


        public GrupoService(GruposDbContext context, ILogger<GrupoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GrupoDetalleResponse>> GetGruposParaUsuarioAsync(string usuarioActualId) {

            _logger.LogInformation("Buscando grupos para el usuario {UsuarioId}", usuarioActualId);

            // Obtener entidades de la BD
            var gruposEntidad = await _context.Grupos
                .Where(g => g.MiembrosGrupos.Any(m => m.UsuarioId == usuarioActualId))
                .Include(g => g.MiembrosGrupos)
                .ToListAsync();

            // Simulacion placeholder de llamada a servicio de Usuarios
            // (En el futuro, esto sería una llamada con _httpClientFactory)
            var usuariosPlaceholder = new Dictionary<string, string>
            {
                { "auth0|id_del_usuario_1", "Usuario Falso 1" },
                { "auth0|id_del_usuario_2", "Usuario Falso 2" },
                { usuarioActualId, "Pepe Lopez" }
            };

            // Mapping a DTOs
            var gruposResponse = new List<GrupoDetalleResponse>();

            foreach (var grupo in gruposEntidad)
            {
                // Mapeo de la lista de miembros
                var miembrosResponse = new List<UsuarioResumenResponse>();
                foreach (var miembro in grupo.MiembrosGrupos)
                {
                    miembrosResponse.Add(new UsuarioResumenResponse(
                        miembro.UsuarioId,
                        usuariosPlaceholder.GetValueOrDefault(miembro.UsuarioId, "Usuario Desconocido")
                    ));
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
            }

            // Agregar todo a la BD en una transacción
            await _context.Grupos.AddAsync(nuevoGrupo);
            await _context.MiembrosGrupos.AddRangeAsync(listaMiembros);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Grupo {GrupoId} creado exitosamente", nuevoGrupo.Id);

            // Mapear a DTO placeholder usuarios api
            var usuariosPlaceholder = new Dictionary<string, string>
            {
                { usuarioActualId, "Pepe Lopez" }
            };

            var miembrosResponse = listaMiembros.Select(m => new UsuarioResumenResponse(
                m.UsuarioId,
                usuariosPlaceholder.GetValueOrDefault(m.UsuarioId, "Usuario Invitado")
            )).ToList();

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

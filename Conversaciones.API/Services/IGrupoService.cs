using Conversaciones.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Conversaciones.API.Services
{
    public interface IGrupoService
    {
        Task<List<GrupoDetalleResponse>> GetGruposParaUsuarioAsync(string usuarioActual);

        Task<ActionResult<GrupoDetalleResponse>> CrearGrupoAsync(CrearGrupoRequest request, string usuarioActualId);
    }
}

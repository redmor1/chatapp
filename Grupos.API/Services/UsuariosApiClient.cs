using Grupos.API.DTOs;
using System.Text.Json;

namespace Grupos.API.Services;

public class UsuariosApiClient : IUsuariosApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsuariosApiClient> _logger;

    public UsuariosApiClient(HttpClient httpClient, ILogger<UsuariosApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<UsuarioResumenResponse>> GetUsuariosBatchAsync(List<string> usuarioIds)
    {
        try
        {
            var request = new { ids = usuarioIds };
            var response = await _httpClient.PostAsJsonAsync("/api/v1/usuarios/batch", request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error al obtener usuarios en batch: {StatusCode}", response.StatusCode);
                return new List<UsuarioResumenResponse>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var usuarios = JsonSerializer.Deserialize<List<UsuarioResumenResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return usuarios ?? new List<UsuarioResumenResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al llamar a Usuarios.API");
            return new List<UsuarioResumenResponse>();
        }
    }
}

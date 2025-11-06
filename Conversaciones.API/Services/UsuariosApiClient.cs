using Conversaciones.API.DTOs;
using System.Text.Json;

namespace Conversaciones.API.Services;

public class UsuariosApiClient : IUsuariosApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsuariosApiClient> _logger;
    private readonly IConfiguration _configuration;

    public UsuariosApiClient(HttpClient httpClient, ILogger<UsuariosApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<UsuarioResumenResponse>> GetUsuariosBatchAsync(List<string> usuarioIds)
    {
        if (!usuarioIds.Any())
        {
            _logger.LogWarning("GetUsuariosBatchAsync llamado con lista vacía");
            return new List<UsuarioResumenResponse>();
        }

        try
        {
            var request = new { ids = usuarioIds };
            
            // Obtener y agregar API Key del header
            var apiKey = _configuration["Services:InterServiceApiKey"];
            _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            
            _logger.LogInformation("Llamando a POST /api/v1/usuario/batch con {Count} IDs", usuarioIds.Count);
            
            var response = await _httpClient.PostAsJsonAsync("/api/v1/usuario/batch", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error en Usuarios.API: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                return new List<UsuarioResumenResponse>();
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta de Usuarios.API: {Content}", content);
            
            var usuarios = JsonSerializer.Deserialize<List<UsuarioResumenResponse>>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation("Obtenidos {Count} usuarios de Usuarios.API", usuarios?.Count ?? 0);
            
            return usuarios ?? new List<UsuarioResumenResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios en batch");
            return new List<UsuarioResumenResponse>();
        }
    }
}

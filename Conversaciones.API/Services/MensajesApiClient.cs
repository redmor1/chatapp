using System.Text;
using System.Text.Json;

namespace Conversaciones.API.Services
{
    public class MensajesApiClient : IMensajesApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MensajesApiClient> _logger;

        public MensajesApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<MensajesApiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task NotificarCambioMembresiaAsync(string tipo, string conversacionId, string usuarioId, string? nombreGrupo)
        {
            var apiKey = _configuration["Services:InterServiceApiKey"];
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/notificaciones/membresia");
            request.Headers.Add("X-API-Key", apiKey);

            var payload = new
            {
                Tipo = tipo,
                ConversacionId = conversacionId,
                UsuarioId = usuarioId,
                NombreGrupo = nombreGrupo
            };

            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al notificar cambio de membresía a Mensajes.API: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al notificar cambio de membresía a Mensajes.API");
            }
        }
    }
}

using System.Net.Http.Headers;

namespace Mensajes.API.Services
{
    public class ConversacionesApiClient : IConversacionesApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConversacionesApiClient> _logger;

        public ConversacionesApiClient(HttpClient httpClient, ILogger<ConversacionesApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> ValidarMembresiaAsync(string conversacionId, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync($"api/v1/conversacion/{conversacionId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar membresía para conversación {ConversacionId}", conversacionId);
                return false;
            }
        }
    }
}

namespace Mensajes.API.Services
{
    public interface IConversacionesApiClient
    {
        Task<bool> ValidarMembresiaAsync(string conversacionId, string token);
    }
}

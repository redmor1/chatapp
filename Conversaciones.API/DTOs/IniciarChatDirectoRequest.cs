namespace Conversaciones.API.DTOs;

public record IniciarChatDirectoRequest(
    [Required]
    [EmailAddress]
    string EmailUsuario
);

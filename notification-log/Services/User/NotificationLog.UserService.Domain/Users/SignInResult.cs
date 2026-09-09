namespace NotificationLog.UserService.Domain.Users;

/// <summary>
/// Resultado de un intento de autenticación. Es un enum y no una excepción porque un login
/// fallido es un desenlace previsto del caso de uso, no un error del programa.
/// </summary>
/// <remarks>
/// El detalle es para el servidor, no para el cliente: la API debe responder lo mismo ante
/// <see cref="InvalidCredentials"/>, <see cref="Inactive"/> y usuario inexistente, porque
/// distinguirlos permite a un atacante enumerar qué correos están dados de alta. Sirve para el
/// log y para decidir si conviene reenviar el correo de verificación.
/// </remarks>
public enum SignInResult
{
    Success,
    InvalidCredentials,
    Inactive,
    EmailNotConfirmed
}

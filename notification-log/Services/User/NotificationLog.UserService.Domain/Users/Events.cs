using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users;

// Los eventos son el modelo de datos real de este servicio: lo que se escribe en Marten es
// esto y nada más, así que su forma es un contrato tan serio como el de una tabla o un .proto.
// Cuatro reglas que se siguen en todo el archivo:
//
// 1. Nombres en pasado y en el lenguaje del negocio ("EmailConfirmed", no "UserUpdated"): un
//    evento cuenta qué pasó, no qué campos cambiaron. Renombrar un tipo obliga a mapear los
//    eventos viejos, así que el nombre se elige una vez y se respeta.
// 2. Ningún evento lleva el id del usuario. La identidad es el id del flujo, que el store
//    garantiza y devuelve en los metadatos de cada evento (IEvent.StreamId); repetirla en el
//    payload solo añade una forma de que el evento contradiga al flujo que lo contiene. Quien
//    necesite el id al publicar hacia fuera lo toma de ahí.
// 3. Solo tipos primitivos, nunca Value Objects. El evento se guarda como JSON y se releerá
//    dentro de años: un cambio en el constructor de Email o Phone no puede romper la lectura
//    de la historia. La reconstrucción de los VO se hace al aplicar, con las fábricas
//    "FromStorage", que no revalidan.
// 4. Tampoco se declara la fecha: la aporta DomainEvent. Duplicar un OccurredAt propio en cada
//    record solo crea dos relojes que pueden discrepar.

/// <summary>Alta del usuario. Es siempre el primer evento del flujo.</summary>
/// <remarks>
/// El teléfono viaja como cadena vacía cuando el usuario se registra sin teléfono, en lugar de
/// como null: mismo criterio que el contrato UserContactUpdated del servicio de notificaciones
/// (vacío = "no hay valor", nunca "no cambia"), para que traducir de uno a otro sea directo.
/// El hash de contraseña se guarda en el evento porque las credenciales forman parte del estado
/// del agregado; lo que jamás entra en un evento es la contraseña en claro.
/// </remarks>
public sealed record UserRegistered(
    string Name,
    string Email,
    string Phone,
    string PasswordHash
) : DomainEvent;

public sealed record UserNameChanged(string Name) : DomainEvent;

/// <summary>
/// El usuario pidió cambiar su correo. Al aplicarse tumba la confirmación previa: un correo
/// nuevo nunca está verificado, y esa consecuencia se deduce del evento en vez de emitir
/// además un "EmailUnconfirmed" que sería redundante.
/// </summary>
public sealed record EmailChanged(string Email) : DomainEvent;

/// <summary>
/// El usuario demostró que controla ese buzón. El código de un solo uso vive en Redis con su
/// TTL y nunca se persiste aquí: al store solo llega el hecho consumado, que es lo único que
/// sigue siendo cierto pasado el TTL.
/// </summary>
/// <remarks>
/// Lleva el correo confirmado —aunque el agregado ya lo tenga— porque el evento cruza la cola
/// hacia el servicio de notificaciones: si solo llevara el hecho, el productor tendría que ir a
/// buscar la dirección al agregado y podría leer una más nueva que la que se verificó,
/// publicando como confirmada una dirección que nadie ha confirmado.
/// </remarks>
public sealed record EmailConfirmed(string Email) : DomainEvent;

public sealed record PhoneChanged(string Phone) : DomainEvent;

public sealed record PhoneConfirmed(string Phone) : DomainEvent;

/// <summary>Cambio de credenciales. Viaja el hash ya calculado, nunca la contraseña.</summary>
/// <remarks>
/// Un store de eventos es inmutable, así que los hashes antiguos quedan en la historia para
/// siempre. Es aceptable porque un hash con sal y coste alto no es un secreto reutilizable,
/// pero conviene tenerlo presente: si algún día hay que borrar de verdad las credenciales de
/// un usuario (derecho al olvido), la solución no es editar eventos sino archivar el flujo.
/// </remarks>
public sealed record PasswordChanged(string PasswordHash) : DomainEvent;

/// <summary>
/// Baja lógica. El motivo es libre y opcional porque en la práctica lo escribe un
/// administrador, pero queda en la historia, que es justo lo que se le suele pedir al soporte.
/// </summary>
public sealed record UserDeactivated(string? Reason) : DomainEvent;

/// <summary>
/// Alta de nuevo tras una baja. No lleva ningún campo: el hecho, el flujo al que pertenece y
/// la fecha que aporta DomainEvent son toda la información que hay que dar.
/// </summary>
public sealed record UserReactivated : DomainEvent;

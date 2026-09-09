using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;

namespace NotificationLog.UserService.Domain.Users;

/// <summary>
/// El usuario, modelado como agregado event-sourced: su estado no se guarda en ninguna parte,
/// se deduce reproduciendo el flujo de eventos de <c>Events.cs</c>.
/// </summary>
/// <remarks>
/// <para>
/// La clase está partida en dos mitades que no se mezclan nunca, y esa separación es lo que
/// permite enchufar Marten desde fuera sin que el dominio lo sepa:
/// </para>
/// <list type="bullet">
/// <item><b>Decidir</b> — los métodos públicos de negocio. Validan contra el estado actual y,
/// si la operación procede, llaman a <c>Raise</c>. Pueden lanzar <see cref="DomainException"/>.</item>
/// <item><b>Evolucionar</b> — los métodos <c>When</c> privados a los que despacha
/// <see cref="Apply"/>. Solo asignan campos. No validan, no consultan nada y no fallan: se
/// ejecutan también al releer historia de hace años, y un evento ya escrito no se puede rechazar.</item>
/// </list>
/// <para>
/// Toda la mecánica de event sourcing —encolar lo decidido, rehidratar desde la historia— la
/// pone <see cref="AggregateRoot"/>. Aquí lo único específico es <see cref="Apply"/>. Se evita a
/// propósito la convención de Marten de declarar métodos <c>Apply(TEvento)</c> públicos por
/// evento: funciona, pero ata la forma del dominio a una librería concreta, y con una proyección
/// de un solo flujo que llame a este <c>Apply</c> se consigue lo mismo dejando el proyecto sin
/// una sola referencia de paquete.
/// </para>
/// </remarks>
public sealed class User : AggregateRoot
{
    public const int NameMaxLength = 120;

    // Constructor vacío para rehidratar: un agregado event-sourced nunca se construye con su
    // estado, nace vacío y se llena aplicando eventos. Es privado — lo invoca AggregateRoot.
    // Rehydrate por reflexión — para que nadie pueda crear un usuario inválido desde fuera.
    private User() { }

    // Constructor del alta: la identidad es lo único que no puede salir de los eventos, porque
    // es la identidad del propio flujo al que se van a escribir.
    private User(Guid id) : base(id) { }

    public string Name { get; private set; } = string.Empty;

    // Email y Password se marcan null! porque están garantizados en cuanto se aplica
    // UserRegistered, que es siempre el primer evento del flujo, y no hay forma pública de
    // obtener un User que no haya pasado por ahí.
    public Email Email { get; private set; } = null!;

    // El teléfono es opcional: un usuario puede darse de alta solo con correo y añadirlo luego.
    public Phone? Phone { get; private set; }

    public PasswordHash Password { get; private set; } = null!;

    public bool IsEmailConfirmed { get; private set; }

    public bool IsPhoneConfirmed { get; private set; }

    public bool IsActive { get; private set; }

    // ---------------------------------------------------------------------------------------
    // Decidir
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Da de alta al usuario. El identificador lo elige quien llama (no la base de datos)
    /// porque en event sourcing el Id <em>es</em> el identificador del flujo y hace falta antes
    /// de poder escribir el primer evento.
    /// </summary>
    /// <remarks>
    /// Que el correo no esté repetido no se comprueba aquí: es una invariante del conjunto de
    /// usuarios y ningún agregado puede verla desde dentro (ver <see cref="IUserRepository"/>).
    /// El usuario nace activo y sin confirmar: puede entrar en el sistema, pero todavía no es
    /// un destinatario válido para notificaciones.
    /// </remarks>
    public static User Register(
        Guid id,
        string name,
        Email email,
        Phone? phone,
        string plainPassword,
        IPasswordHasher hasher)
    {
        if (id == Guid.Empty)
            throw new DomainException("El identificador del usuario es obligatorio.");

        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(hasher);

        var user = new User(id);

        user.Raise(new UserRegistered(
            NormalizeName(name),
            email.Value,
            // Cadena vacía en lugar de null cuando no hay teléfono: el evento describe un
            // estado completo, y "vacío" ya significa "sin valor" en todo el sistema.
            phone?.Value ?? string.Empty,
            PasswordHash.Create(plainPassword, hasher).Value));

        return user;
    }

    public void ChangeName(string name)
    {
        EnsureActive();

        var normalized = NormalizeName(name);

        // Un cambio que no cambia nada no genera evento: el flujo es la historia del usuario y
        // llenarlo de eventos idénticos encarece cada relectura sin aportar información.
        if (normalized == Name)
            return;

        Raise(new UserNameChanged(normalized));
    }

    /// <summary>
    /// Cambia el correo. La confirmación anterior se pierde al aplicar el evento: la dirección
    /// nueva no la ha demostrado nadie todavía.
    /// </summary>
    public void ChangeEmail(Email email)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(email);

        if (email == Email)
            return;

        Raise(new EmailChanged(email.Value));
    }

    /// <summary>
    /// Registra que el usuario demostró controlar su correo. La comprobación del código de un
    /// solo uso ocurre <em>antes</em>, en la capa de aplicación, contra Redis; el dominio no
    /// sabe nada de códigos ni de TTL, solo recibe el hecho ya verificado.
    /// </summary>
    /// <param name="confirmed">
    /// Dirección que se verificó. Se pide como parámetro en vez de dar por buena la actual para
    /// cerrar la carrera obvia: si el usuario cambió de correo entre que pidió el código y lo
    /// introdujo, ese código no puede confirmar la dirección nueva.
    /// </param>
    public void ConfirmEmail(Email confirmed)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(confirmed);

        if (confirmed != Email)
            throw new DomainException(
                "El correo verificado ya no es el correo actual del usuario; solicita una nueva verificación.");

        // Idempotente a propósito: el usuario puede pulsar dos veces el enlace y el consumidor
        // de mensajes tiene entrega "al menos una vez". Confirmar lo ya confirmado no es un
        // error, simplemente no ha pasado nada nuevo que contar.
        if (IsEmailConfirmed)
            return;

        Raise(new EmailConfirmed(Email.Value));
    }

    public void ChangePhone(Phone phone)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(phone);

        if (phone == Phone)
            return;

        Raise(new PhoneChanged(phone.Value));
    }

    public void ConfirmPhone(Phone confirmed)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(confirmed);

        if (Phone is null)
            throw new DomainException("El usuario no tiene un teléfono que verificar.");

        if (confirmed != Phone)
            throw new DomainException(
                "El teléfono verificado ya no es el teléfono actual del usuario; solicita una nueva verificación.");

        if (IsPhoneConfirmed)
            return;

        Raise(new PhoneConfirmed(Phone.Value));
    }

    /// <summary>
    /// Cambia la contraseña comprobando primero la actual, que es lo que impide que alguien con
    /// una sesión robada se apropie de la cuenta.
    /// </summary>
    public void ChangePassword(string currentPassword, string newPlainPassword, IPasswordHasher hasher)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(hasher);

        if (!Password.Matches(currentPassword, hasher))
            throw new DomainException("La contraseña actual no es correcta.");

        Raise(new PasswordChanged(PasswordHash.Create(newPlainPassword, hasher).Value));
    }

    /// <summary>
    /// Restablece la contraseña sin conocer la anterior. Solo debe llamarse cuando la capa de
    /// aplicación ya consumió en Redis un token de recuperación válido: es el mismo patrón que
    /// la confirmación de correo, el dominio recibe el hecho y no el token.
    /// </summary>
    public void ResetPassword(string newPlainPassword, IPasswordHasher hasher)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(hasher);

        Raise(new PasswordChanged(PasswordHash.Create(newPlainPassword, hasher).Value));
    }

    /// <summary>
    /// Decide si un intento de login es válido. No genera ningún evento a propósito: un login
    /// correcto no cambia el estado del usuario, y escribir uno por cada entrada convertiría el
    /// flujo en un log de accesos que hay que releer entero en cada operación. El rastro de
    /// accesos y el contador de intentos fallidos van en Redis, junto a los códigos de un solo
    /// uso, donde caducan solos.
    /// </summary>
    public SignInResult TrySignIn(string plainPassword, IPasswordHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        // La contraseña se comprueba antes que el estado de la cuenta: responder "cuenta
        // desactivada" a quien no ha acertado la contraseña le confirma que ese correo existe.
        if (!Password.Matches(plainPassword, hasher))
            return SignInResult.InvalidCredentials;

        if (!IsActive)
            return SignInResult.Inactive;

        if (!IsEmailConfirmed)
            return SignInResult.EmailNotConfirmed;

        return SignInResult.Success;
    }

    /// <summary>
    /// Baja lógica. Nunca se borra el flujo: la historia del usuario es justo lo que se quiere
    /// conservar, y además el servicio de notificaciones necesita enterarse para dejar de
    /// enviarle mensajes a su réplica Recipient.
    /// </summary>
    public void Deactivate(string? reason = null)
    {
        if (!IsActive)
            return;

        Raise(new UserDeactivated(string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()));
    }

    // Sin EnsureActive: es la única operación que tiene sentido sobre un usuario dado de baja.
    public void Reactivate()
    {
        if (IsActive)
            return;

        Raise(new UserReactivated());
    }

    // ---------------------------------------------------------------------------------------
    // Evolucionar
    // ---------------------------------------------------------------------------------------

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserRegistered e: When(e); break;
            case UserNameChanged e: When(e); break;
            case EmailChanged e: When(e); break;
            case EmailConfirmed e: When(e); break;
            case PhoneChanged e: When(e); break;
            case PhoneConfirmed e: When(e); break;
            case PasswordChanged e: When(e); break;
            case UserDeactivated: IsActive = false; break;
            case UserReactivated: IsActive = true; break;

            // Un evento desconocido se rechaza en vez de ignorarse en silencio: casi siempre
            // significa que se escribió en el flujo equivocado o que falta registrar el mapeo
            // de un evento renombrado, y descubrirlo al leer es mucho mejor que arrastrar un
            // agregado con el estado a medias.
            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado User.");
        }
    }

    private void When(UserRegistered e)
    {
        Name = e.Name;
        Email = Email.FromStorage(e.Email);
        Phone = string.IsNullOrEmpty(e.Phone) ? null : Phone.FromStorage(e.Phone);
        Password = PasswordHash.FromStorage(e.PasswordHash);
        IsEmailConfirmed = false;
        IsPhoneConfirmed = false;
        IsActive = true;
    }

    private void When(UserNameChanged e) => Name = e.Name;

    private void When(EmailChanged e)
    {
        Email = Email.FromStorage(e.Email);
        IsEmailConfirmed = false;
    }

    private void When(EmailConfirmed e)
    {
        // Se reasigna el correo del evento, en lugar de solo poner el flag: así el resultado no
        // depende del estado previo y aplicar la historia dos veces da lo mismo.
        Email = Email.FromStorage(e.Email);
        IsEmailConfirmed = true;
    }

    private void When(PhoneChanged e)
    {
        Phone = Phone.FromStorage(e.Phone);
        IsPhoneConfirmed = false;
    }

    private void When(PhoneConfirmed e)
    {
        Phone = Phone.FromStorage(e.Phone);
        IsPhoneConfirmed = true;
    }

    private void When(PasswordChanged e) => Password = PasswordHash.FromStorage(e.PasswordHash);

    // ---------------------------------------------------------------------------------------

    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("El usuario está desactivado.");
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del usuario es obligatorio.");

        var trimmed = name.Trim();

        // Se corta en vez de lanzar, igual que hace Recipient en el servicio de notificaciones:
        // un nombre largo es un dato feo, no una violación de negocio que deba tumbar el alta.
        return trimmed.Length > NameMaxLength ? trimmed[..NameMaxLength] : trimmed;
    }
}

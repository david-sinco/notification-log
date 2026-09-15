# Servicio Identity — referencia de diseño

> Documento de referencia para implementar el servicio en el futuro. **Nada de lo descrito aquí está
> implementado todavía**; la sección [Estado actual del código](#9-estado-actual-del-código) dice qué
> existe hoy y qué se reutiliza.

---

## 1. Contexto de negocio

La plataforma es un portal inmobiliario con dos tipos de actividad:

- **Publicar:** personas que quieren arrendar o vender su vivienda la suben y la publican.
- **Buscar:** personas que ven lo publicado, lo guardan en favoritos, piden visitas, etc.

Publicar y buscar **no son tipos de cuenta**: cualquier usuario puede hacer las dos cosas. El único
tipo especial de usuario es el **asesor**.

Servicios de la plataforma:

| Servicio | Responsabilidad |
|---|---|
| **Identity** (este) | quién es cada persona, sus cuentas de acceso y sus perfiles de asesor |
| **Publicaciones** (futuro) | inmuebles, publicación, favoritos, búsquedas, visitas y el resto de lógica del portal |
| **Notification** (existente) | envío de notificaciones por correo, SMS, push y WhatsApp |

## 2. Responsabilidad del servicio

Identity responde a una sola pregunta, **"¿quién es?"**, en tres niveles:

| Nivel | Agregado | Responde |
|---|---|---|
| Identidad de los datos | `Person` | quién es esta persona y qué datos suyos están verificados |
| Identidad de acceso | `User` | con qué cuenta entra y qué sesiones tiene abiertas |
| Identidad profesional | `Advisor` | si además es asesor y en qué condiciones trabaja |

Además guarda la prueba de los consentimientos de tratamiento de datos (`ConsentLedger`).

**Por qué centralizar a las personas en un servicio propio:**

- **Una sola identidad:** el mismo `PersonId` en todos los servicios.
- **Se verifica una sola vez:** un correo o teléfono verificado sirve para toda la plataforma.
- **Un solo consentimiento:** la revocación aplica en todas partes a la vez (Ley 1581, Habeas Data).
- **Un cambio se propaga a todos:** los demás servicios mantienen réplicas alimentadas por eventos.
- **Una sola puerta para el derecho al olvido.**
- **Unicidad de los identificadores** en toda la plataforma.

**Lo que NO es de Identity** (va en Publicaciones): favoritos, búsquedas guardadas y alertas, avisos de
baja de precio, historial de vistos, comparador, notas privadas sobre un inmueble, solicitudes de visita,
contacto con el publicador o asesor, reportes de publicaciones.

Identity aporta a esas funciones los contactos verificados, las preferencias de notificación, los
consentimientos y el **nivel de verificación** de cada persona. Con él, Publicaciones puede exigir, por
ejemplo, teléfono verificado para contactar a un publicador o documento verificado para publicar.

## 3. Nombre y ubicación

- Servicio: **Identity**
- Carpeta: `Services/Identity/`
- Proyectos: `NotificationLog.IdentityService.<Capa>`

Referencia: el contexto *Identity and Access* de *Implementing Domain-Driven Design* (Vaughn Vernon).

## 4. Decisiones de diseño

### Persistencia y agregados

- **Event sourcing con Marten (PostgreSQL)** para todos los agregados. El dominio no conoce Marten.
- Todos los agregados heredan de `Domain.Shared.Common.AggregateRoot`:
  - `Raise(evento)` aplica el evento y lo deja pendiente de guardar.
  - `Apply(evento)` es la única transición de estado: no valida ni tiene efectos secundarios.
  - `AggregateRoot.Rehydrate<T>(id, historia)` reconstruye el agregado. Lanza `ArgumentException` si la
    historia está vacía; el repositorio debe devolver `null` cuando el flujo no existe.
- **Constructor privado y sin parámetros** en cada agregado, usado solo al rehidratar. Para crear se
  usa una **fábrica estática** que valida y aplica el evento de creación.
- Los value objects tienen `Create` (valida y normaliza) y un `FromStorage` interno que no revalida,
  para que endurecer una regla mañana no impida releer los eventos de ayer.
- **La versión del flujo no vive en el dominio.** El control de concurrencia lo hace Marten al guardar.

### Eventos

- Nombres en pasado y en lenguaje de negocio.
- **Solo el evento de creación lleva el id del agregado.** El resto lo toma del flujo (`StreamId`).
- Solo tipos primitivos, nunca value objects.
- La fecha no se declara: la aporta `DomainEvent`.
- Un tipo por archivo, en la carpeta `Events/` del agregado, con namespace `...<Agregado>.Events`.
- **Granularidad:**
  - Un evento por hecho cuando cada cambio tiene consecuencias distintas. Ejemplo: cambiar un
    identificador anula su verificación.
  - Un evento con el estado completo cuando quien lo consume siempre reevalúa el conjunto. Ejemplos:
    `ContactPreferencesChanged`, `AdvisorProfileUpdated`.

### Identificación de personas

- **Tres identificadores: correo, teléfono y documento.** Los tres son únicos en toda la plataforma,
  se añaden progresivamente y en cualquier orden, y siempre tiene que existir al menos uno.
- Se modelan como un solo value object, `PersonIdentifier(Type, Value)`, con `Type = Email | Phone |
  Document`. `Email`, `Phone` e `IdentityDocument` siguen validando cada formato por separado.
- **Eventos de identificador genéricos**: `IdentifierAdded(Type, …)` y similares, en lugar de uno por
  tipo. *Elegido por defecto, revisable.* El coste es que un consumidor interesado solo en el correo
  tiene que filtrar por `Type`.
- **Sin fusión de duplicados.** Si una misma persona se registró dos veces con identificadores
  distintos, añadir el segundo identificador falla ("este correo ya pertenece a otra persona") y el
  caso se resuelve a mano desde soporte.

### Unicidad

- La unicidad es una regla sobre el conjunto de personas; ningún agregado puede comprobarla solo.
- Se resuelve con **reservas en el repositorio de escritura**: `TryReserveIdentifierAsync` y
  `ReleaseIdentifierAsync`.
- En Marten, la reserva es un documento cuyo `Id` es el identificador normalizado; la clave primaria
  rechaza el duplicado.
- **Contrato:** las reservas y los eventos se guardan en la **misma sesión**, y
  `IUnitOfWork.SaveChangesAsync` los confirma juntos. Así no quedan reservas huérfanas si algo falla.

### Cuentas y sesiones

- `User` es la cuenta de acceso, con relación 1 a 1 con su `Person`.
- Se inicia sesión con **cualquier correo o teléfono verificado** de la persona. `User` no guarda el
  identificador.
- **Las sesiones no van en el flujo de eventos.** Dispositivo, refresh token y última actividad cambian
  constantemente y caducan solos, así que viven en Redis con TTL.
- **Un login correcto no genera evento.** Solo quedan en el flujo los hechos de seguridad auditables:
  bloqueo, cambio de contraseña, rotación del *security stamp*.
- **Security stamp:** es un valor que viaja dentro de cada sesión o token. Rotarlo (al cambiar la
  contraseña o bloquear la cuenta) invalida todas las sesiones abiertas de una vez.
- El hash de contraseña lo calcula `IPasswordHasher`, en Application. El agregado recibe el
  `PasswordHash` ya calculado y la política de contraseñas es `PasswordPolicy`, en Domain.

### Asesores

- **"Un asesor es un usuario, pero un usuario no es un asesor"** se modela con composición, no con
  herencia. `Advisor` es un agregado propio con el **mismo Id que su `User`**.
- No se usa herencia porque un usuario se convierte en asesor tiempo después de registrarse, y porque la
  cuenta y la actividad profesional tienen ciclos de vida distintos: suspender a un asesor no bloquea su
  cuenta.
- El token de sesión incluye el permiso de asesor solo si el `Advisor` está `Active`.

### Consentimientos y anonimización

- Los consentimientos son un agregado aparte, `ConsentLedger`, con un flujo por persona **sin datos
  personales**: solo `PersonId`, finalidad, versión de la política, origen y fechas. Así la prueba de
  consentimiento sobrevive a la anonimización.
- Anonimizar a una persona: `PersonAnonymized` deja el estado de `Person` sin datos personales y como
  final, deshabilita el `User` y retira el `Advisor`, todo en la misma operación.
- **Los eventos anteriores siguen conteniendo datos personales.** Se resuelve en infraestructura, con
  borrado criptográfico (datos cifrados con una clave por persona que se destruye) o con el
  enmascaramiento de eventos de Marten. El dominio debe dejar identificado qué campos de qué eventos son
  datos personales.

### Application y CQRS

- **Estándar de Notification:** una carpeta por caso de uso con `XCommand` (record), `XHandler`
  (registrado a mano en `DependencyInjection.cs`) y `XValidator` (FluentValidation, `internal sealed`).
- **Un comando por formulario**, que puede emitir varios eventos. El número de comandos lo decide la UI;
  el de eventos, lo que el negocio necesita saber después.
- Todo handler llama a `AppendAsync` antes de `SaveChangesAsync`, porque en event sourcing los cambios no
  se detectan solos.
- El código genérico compartido entre servicios va en **`Application.Shared`**: `IUnitOfWork`,
  `AppValidationException`, `NotFoundException`, `ValidatorExtensions` y, cuando haga falta,
  `PagedResult`.
- **CQRS con proyectos separados de escritura y lectura:**
  - **Regla:** si el dato sirve para decidir, va por escritura, contra el flujo y con consistencia
    fuerte (el login, las reservas). Si sirve para mostrarse, va por lectura.
  - **Application.Read** nunca carga agregados. Solo usa DTOs y los puertos de consulta.
  - **Proyecciones:** la lógica es una función pura en Application.Read (vista + evento → vista nueva) y
    la alimenta Marten desde Infrastructure.Read. Se empieza con proyecciones inline, que actualizan la
    vista en la misma transacción que los eventos.
  - **Una API ahora**, que llama a `AddIdentityWrite()` y `AddIdentityRead()`. Cuando haga falta escalar,
    una `Api.Read` que llama solo a `AddIdentityRead()`. El daemon de Marten corre en un único proceso, y
    el lado de lectura puede apuntar a una réplica de solo lectura de PostgreSQL.

### Convenciones de código

- Un tipo por archivo.
- El namespace sigue la ruta de carpetas.
- Sin comentarios en el código salvo que se pidan.
- Los enums van en la raíz de la carpeta del agregado, como `DeliveryStatus` en Notification; los value
  objects en `ValueObjects/` y los eventos en `Events/`.
- Handlers con campos `private readonly` asignados por tupla en el constructor, y método
  `HandleAsync(cmd, ct)`.

## 5. Agregados

### 5.1 `Person`

**Datos:**
- tipo (natural o jurídica)
- nombre (`PersonName` para personas naturales, `LegalName` para jurídicas)
- identificadores con su estado de verificación
- direcciones por tipo (residencia y notificación)
- preferencias de contacto (canal preferido, idioma, zona horaria y horario en que se le puede
  contactar)
- representante legal (solo personas jurídicas; es otra `Person`)
- estado

**Estados:** `Active | Anonymized`.

**Eventos:** `PersonRegistered`, `IdentifierAdded`, `IdentifierReplaced`,
`IdentifierVerified(Type, Method, VerifiedBy)`, `IdentifierRemoved`, `PersonNameChanged`,
`AddressChanged`, `ContactPreferencesChanged`, `LegalRepresentativeAssigned`, `PersonAnonymized`.

**Reglas:**
- Se registra con al menos un identificador, y nunca se puede quitar el último.
- Cada identificador es único en la plataforma (reserva en el repositorio).
- Reemplazar un identificador anula su verificación.
- El correo y el teléfono se verifican con un código (`VerificationMethod.Code`). El documento se
  verifica por revisión (`VerificationMethod.DocumentReview`), y queda registrado quién lo revisó y
  cuándo.
- El tipo de documento tiene que corresponder al tipo de persona: NIT solo para jurídicas; CC, CE o
  pasaporte solo para naturales.
- **Representante legal:** solo lo tienen las personas jurídicas y no puede ser la propia persona. Que
  el representante sea una persona natural y esté activa lo comprueba Application antes de llamar al
  dominio.
- El canal preferido tiene que ser un identificador verificado.
- Si la persona tiene una cuenta activa, debe conservar al menos un correo o teléfono verificado, porque
  es con lo que inicia sesión. Lo comprueba Application antes de quitar un identificador.
- Una persona anonimizada queda en estado final y rechaza cualquier comando.

### 5.2 `User`

**Datos:** `PersonId`, estado, hash de contraseña y security stamp.

**Estados:** `Active | Locked | Disabled`.

**Eventos:** `UserCreated(UserId, PersonId)`, `PasswordChanged`, `UserLocked(Reason, Until)`,
`UserUnlocked`, `UserDisabled`, `UserEnabled`, `SecurityStampRotated`.

**Reglas:**
- Cada persona tiene como máximo una cuenta.
- Para crear la cuenta, la persona necesita al menos un correo o teléfono verificado.
- Varios intentos fallidos seguidos generan `UserLocked`. Un login correcto no genera evento.
- Cambiar la contraseña o bloquear la cuenta rota el security stamp, y con eso se invalidan todas las
  sesiones.
- Una cuenta bloqueada o deshabilitada no puede iniciar sesión.

### 5.3 `Advisor`

**Id:** el mismo que el del `User`.

**Datos:**
- **Perfil público (`AdvisorProfile`):** biografía, foto, años de experiencia, idiomas, capacidad
  (cuántos inmuebles puede gestionar a la vez) y canal de contacto público.
- **Especialidades:** `Arriendo | Venta | Comercial`.
- **Zonas de servicio (`ServiceArea`):** ciudad y barrio.
- **Disponibilidad:** si acepta clientes nuevos.
- **Afiliación a una inmobiliaria**, que es una `Person` jurídica.

**Estados:** `Pending → Active → Suspended → Retired`. Además `Rejected`: una solicitud rechazada puede
volver a presentarse.

**Eventos:** `AdvisorApplied`, `AdvisorApproved(ReviewedBy)`, `AdvisorRejected(Reason)`,
`AdvisorProfileUpdated`, `ServiceAreasChanged`, `AvailabilityChanged`, `AdvisorAffiliated(AgencyId)`,
`AdvisorSuspended(Reason)`, `AdvisorReinstated`, `AdvisorRetired`.

**Reglas:**
- Para solicitar ser asesor, la cuenta debe estar activa y el documento de la persona verificado. Lo
  comprueba Application con lectura fuerte, porque todo vive en el mismo servicio.
- Solo se aprueba con al menos una zona y una especialidad, y siempre lo aprueba un revisor.
- Un asesor suspendido deja de aceptar clientes automáticamente.
- La inmobiliaria a la que se afilia tiene que ser una persona jurídica (lo comprueba Application).
- Un asesor retirado queda en estado final.

### 5.4 `ConsentLedger`

**Id del flujo:** el `PersonId`.

**Datos:** para cada finalidad, si está autorizada o revocada, con la versión de la política aceptada,
el origen y las fechas. Sin ningún dato personal.

- **Finalidades:** `Comunicaciones | Alertas | CompartirConPublicador`
- **Orígenes:** `Web | Llamada | Presencial`

**Eventos:** `ConsentGranted(Purpose, PolicyVersion, Source)`, `ConsentRevoked(Purpose)`.

**Reglas:**
- Autorizar lo que ya está autorizado, o revocar lo que ya está revocado, no genera evento.
- Toda autorización registra la versión de la política y el origen, que es lo que sirve como prueba.
- El flujo se conserva aunque la persona sea anonimizada.

## 6. Integración con otros servicios

- **Notification:**
  - **Cambios de contacto:** Identity le publica los cambios mediante el outbox de Wolverine (contrato
    `UserContactUpdated`, que podría renombrarse a `PersonContactUpdated`). Solo se envían direcciones
    **verificadas**; una cadena vacía significa "no hay valor verificado".
  - **Códigos de verificación:** se envían con `IVerificationCodeSender`.
- **Publicaciones:**
  - Guarda referencias por `PersonId`, `UserId` y `AdvisorId`.
  - Consume por eventos el nivel de verificación, la tarjeta pública del asesor y los consentimientos.
  - Nunca consulta a Identity de forma síncrona en cada petición.
- **Token de sesión:** incluye el security stamp y el permiso de asesor cuando corresponde.

## 7. Estructura de proyectos

```
Services/Identity/
├── NotificationLog.IdentityService.Domain
├── NotificationLog.IdentityService.Application.Write
├── NotificationLog.IdentityService.Application.Read
├── NotificationLog.IdentityService.Infrastructure.Write
├── NotificationLog.IdentityService.Infrastructure.Read
├── NotificationLog.IdentityService.Api
└── NotificationLog.IdentityService.Tests

Shared/                                    (sin cambios)
├── Domain.Shared
├── Application.Shared
├── NotificationLog.Contracts
└── NotificationLog.ServiceDefaults
```

**Referencias entre proyectos:**

| Proyecto | Referencia a | No puede referenciar |
|---|---|---|
| Domain | Domain.Shared | nada más |
| Application.Write | Domain, Application.Shared | nada de lectura |
| Application.Read | Application.Shared, Domain (solo los eventos, para las proyecciones) | Application.Write |
| Infrastructure.Write | Application.Write, Domain | nada de lectura |
| Infrastructure.Read | Application.Read, Domain (solo los eventos) | Application.Write |
| Api | todos | — |
| Api.Read (futuro) | Application.Read, Infrastructure.Read | Application.Write, Infrastructure.Write |

## 8. Estructura de archivos

### 8.1 Domain

```
NotificationLog.IdentityService.Domain/
├── Persons/
│   ├── Person.cs
│   ├── IPersonRepository.cs               Load, Append, TryReserveIdentifier, ReleaseIdentifier
│   ├── PersonKind.cs                      Natural | Juridica
│   ├── PersonStatus.cs                    Active | Anonymized
│   ├── IdentifierType.cs                  Email | Phone | Document
│   ├── DocumentType.cs                    CC | CE | Pasaporte | NIT
│   ├── AddressType.cs                     Residencia | Notificacion
│   ├── VerificationMethod.cs              Code | DocumentReview
│   ├── Events/
│   │   ├── PersonRegistered.cs
│   │   ├── IdentifierAdded.cs
│   │   ├── IdentifierReplaced.cs
│   │   ├── IdentifierVerified.cs
│   │   ├── IdentifierRemoved.cs
│   │   ├── PersonNameChanged.cs
│   │   ├── AddressChanged.cs
│   │   ├── ContactPreferencesChanged.cs
│   │   ├── LegalRepresentativeAssigned.cs
│   │   └── PersonAnonymized.cs
│   └── ValueObjects/
│       ├── PersonIdentifier.cs
│       ├── Email.cs
│       ├── Phone.cs
│       ├── IdentityDocument.cs
│       ├── PersonName.cs                  nombres y apellidos (persona natural)
│       ├── LegalName.cs                   razón social (persona jurídica)
│       ├── PostalAddress.cs
│       ├── ContactPreferences.cs
│       └── ContactWindow.cs
├── Users/
│   ├── User.cs
│   ├── IUserRepository.cs
│   ├── UserStatus.cs                      Active | Locked | Disabled
│   ├── PasswordPolicy.cs
│   ├── Events/
│   │   ├── UserCreated.cs
│   │   ├── PasswordChanged.cs
│   │   ├── UserLocked.cs
│   │   ├── UserUnlocked.cs
│   │   ├── UserDisabled.cs
│   │   ├── UserEnabled.cs
│   │   └── SecurityStampRotated.cs
│   └── ValueObjects/
│       ├── PasswordHash.cs
│       └── SecurityStamp.cs
├── Advisors/
│   ├── Advisor.cs
│   ├── IAdvisorRepository.cs
│   ├── AdvisorStatus.cs                   Pending | Active | Rejected | Suspended | Retired
│   ├── Specialty.cs                       Arriendo | Venta | Comercial
│   ├── Events/
│   │   ├── AdvisorApplied.cs
│   │   ├── AdvisorApproved.cs
│   │   ├── AdvisorRejected.cs
│   │   ├── AdvisorProfileUpdated.cs
│   │   ├── ServiceAreasChanged.cs
│   │   ├── AvailabilityChanged.cs
│   │   ├── AdvisorAffiliated.cs
│   │   ├── AdvisorSuspended.cs
│   │   ├── AdvisorReinstated.cs
│   │   └── AdvisorRetired.cs
│   └── ValueObjects/
│       ├── AdvisorProfile.cs              bio, foto, idiomas, experiencia, capacidad, canal público
│       └── ServiceArea.cs                 ciudad y barrio
├── Consents/
│   ├── ConsentLedger.cs
│   ├── IConsentLedgerRepository.cs
│   ├── ConsentPurpose.cs                  Comunicaciones | Alertas | CompartirConPublicador
│   ├── ConsentSource.cs                   Web | Llamada | Presencial
│   └── Events/
│       ├── ConsentGranted.cs
│       └── ConsentRevoked.cs
└── Shared/
    └── ContactChannel.cs                  Email | Sms | WhatsApp
```

### 8.2 Application.Write

Cada carpeta de comando contiene `XCommand.cs`, `XHandler.cs` y `XValidator.cs`. Las marcadas con `*` no
llevan validador, como `SetTemplateStatus` en Notification.

```
NotificationLog.IdentityService.Application.Write/
├── Abstractions/
│   ├── IPasswordHasher.cs
│   ├── IVerificationCodeStore.cs
│   ├── IVerificationCodeSender.cs
│   ├── VerificationPurpose.cs             Email | Phone | PasswordReset
│   ├── ISessionStore.cs
│   └── ITokenIssuer.cs
├── Persons/Commands/
│   ├── RegisterPerson/
│   ├── AddIdentifier/
│   ├── ReplaceIdentifier/
│   ├── RemoveIdentifier/
│   ├── RequestIdentifierVerification/     emite el código del correo o del teléfono
│   ├── VerifyIdentifier/                  consume el código
│   ├── VerifyDocument/                    un revisor valida el documento
│   ├── UpdatePersonProfile/               nombre y direcciones
│   ├── UpdateContactPreferences/
│   ├── AssignLegalRepresentative/
│   └── AnonymizePerson/ *                 también deshabilita el User y retira el Advisor
├── Users/Commands/
│   ├── SignUp/                            crea Person y User en una sola operación
│   ├── CreateUser/                        cuenta para una Person que ya existía
│   ├── SignIn/
│   ├── SignOut/ *
│   ├── RefreshSession/
│   ├── ChangePassword/
│   ├── RequestPasswordReset/
│   ├── ResetPassword/
│   ├── SetUserStatus/ *
│   └── UnlockUser/ *
├── Advisors/Commands/
│   ├── ApplyAsAdvisor/
│   ├── ReviewAdvisor/                     aprobar o rechazar
│   ├── UpdateAdvisorProfile/              perfil, especialidades y zonas
│   ├── SetAdvisorAvailability/ *
│   ├── AffiliateAdvisor/
│   └── SetAdvisorStatus/ *                suspender, reincorporar o retirar
├── Consents/Commands/
│   ├── GrantConsent/
│   └── RevokeConsent/
└── DependencyInjection.cs
```

### 8.3 Application.Read

Cada carpeta de consulta contiene `XQuery.cs` y `XHandler.cs`. Los proyectores son funciones puras
(vista + evento → vista nueva); Infrastructure.Read se encarga de pasarles los eventos.

```
NotificationLog.IdentityService.Application.Read/
├── Abstractions/
│   ├── IPersonQueries.cs
│   ├── IAccountQueries.cs
│   └── IAdvisorQueries.cs
├── Persons/
│   ├── Queries/
│   │   ├── GetPersonById/
│   │   └── SearchPersons/                 por identificador o nombre (administración)
│   ├── Dtos/
│   │   ├── PersonDto.cs
│   │   └── PersonSummaryDto.cs
│   └── Projections/
│       ├── PersonView.cs
│       └── PersonViewProjector.cs
├── Users/
│   ├── Queries/
│   │   ├── GetMyAccount/
│   │   └── ListMySessions/
│   └── Dtos/
│       ├── AccountDto.cs
│       └── SessionDto.cs
├── Advisors/
│   ├── Queries/
│   │   ├── GetAdvisorById/
│   │   └── SearchAdvisors/                directorio público por zona, especialidad y disponibilidad
│   ├── Dtos/
│   │   ├── AdvisorDto.cs
│   │   └── AdvisorCardDto.cs              la tarjeta que se muestra en los anuncios
│   └── Projections/
│       ├── AdvisorView.cs
│       └── AdvisorViewProjector.cs
└── DependencyInjection.cs
```

### 8.4 Infrastructure.Write

```
NotificationLog.IdentityService.Infrastructure.Write/
├── Persistence/
│   ├── MartenStoreConfiguration.cs        tipos de evento y flujos
│   ├── MartenUnitOfWork.cs
│   ├── MartenPersonRepository.cs
│   ├── MartenUserRepository.cs
│   ├── MartenAdvisorRepository.cs
│   ├── MartenConsentLedgerRepository.cs
│   └── Reservations/
│       └── IdentifierReservation.cs       documento cuyo Id es el identificador: garantiza la unicidad
├── Security/
│   ├── IdentityPasswordHasher.cs
│   ├── JwtTokenIssuer.cs
│   └── JwtOptions.cs
├── Sessions/
│   └── RedisSessionStore.cs
├── Verification/
│   ├── RedisVerificationCodeStore.cs
│   └── NotificationVerificationCodeSender.cs
├── Messaging/
│   ├── RabbitMqMessagingExtensions.cs
│   └── Publishers/
│       └── PersonContactPublisher.cs      avisa a Notification de los cambios de contacto (outbox de Wolverine)
└── DependencyInjection.cs                 AddIdentityWrite()
```

### 8.5 Infrastructure.Read

```
NotificationLog.IdentityService.Infrastructure.Read/
├── Projections/
│   ├── PersonViewProjection.cs            SingleStreamProjection que delega en PersonViewProjector
│   └── AdvisorViewProjection.cs           SingleStreamProjection que delega en AdvisorViewProjector
├── Queries/
│   ├── MartenPersonQueries.cs
│   ├── MartenAccountQueries.cs            lee la vista de Marten y las sesiones de Redis
│   └── MartenAdvisorQueries.cs
└── DependencyInjection.cs                 AddIdentityRead()
```

### 8.6 Api

```
NotificationLog.IdentityService.Api/
├── Program.cs                             llama a AddIdentityWrite() y AddIdentityRead()
├── appsettings.json
├── Properties/launchSettings.json
├── Endpoints/
│   ├── PersonEndpoints.cs
│   ├── AccountEndpoints.cs                registro, login, sesiones y contraseña
│   ├── AdvisorEndpoints.cs
│   └── ConsentEndpoints.cs
├── Contracts/
│   ├── Persons/                           un request/response por endpoint, como en Notification
│   ├── Accounts/
│   ├── Advisors/
│   └── Consents/
└── Exceptions/
    └── GlobalExceptionHandler.cs          AppValidationException → 400, NotFoundException → 404,
                                           DomainException → 422, cualquier otra → 500
```

### 8.7 Tests

```
NotificationLog.IdentityService.Tests/
├── Domain/
│   ├── PersonTests.cs
│   ├── UserTests.cs
│   ├── AdvisorTests.cs
│   └── ConsentLedgerTests.cs
└── Application/
    ├── Persons/                           handlers con dobles en memoria, sin Docker
    ├── Users/
    └── Advisors/
```

## 9. Estado actual del código

Lo que existe hoy y se reutiliza:

**`Shared/Domain.Shared`**
- `AggregateRoot`: `Raise`, `Apply`, `DomainEvents`, `ClearDomainEvents` y `Rehydrate<T>(id, historia)`
  con validación de historia vacía.
- `Entity`, `DomainEvent`, `IDomainEvent` y `DomainException`.

**`Shared/Application.Shared`**
- `IUnitOfWork`, `AppValidationException`, `NotFoundException` y `ValidatorExtensions`.

**`Services/User/NotificationLog.UserService.Domain`**
- Agregado `User`, event-sourced. Contiene lo que será la base de `Person`: nombre, correo, teléfono,
  sus verificaciones y el estado activo o inactivo.
- Eventos en `Users/Events/`: `UserRegistered`, `UserNameChanged`, `EmailChanged`, `EmailConfirmed`,
  `PhoneChanged`, `PhoneConfirmed`, `UserDeactivated` y `UserReactivated`.
- Value objects en `Users/ValueObjects/`: `Email`, `Phone` y `PersonName`, con `Create` y `FromStorage`.
- `IUserRepository`: `LoadAsync`, `AppendAsync`, `TryReserveEmailAsync` y `ReleaseEmailAsync`.

**`Services/User/NotificationLog.UserService.Application`**
- Comandos: `RegisterUser`, `UpdateProfile`, `ConfirmEmail`, `ConfirmPhone` y `SetUserStatus`.
- Puertos: `IVerificationCodeStore` y `VerificationPurpose` (`Email | Phone`).

Todavía no existen Infrastructure ni Api para este servicio.

**Correspondencia con el diseño nuevo:**

| Hoy | Pasa a |
|---|---|
| `Services/User/NotificationLog.UserService.*` | `Services/Identity/NotificationLog.IdentityService.*` |
| agregado `User` (datos de la persona) | `Person` |
| `Email`, `Phone`, `PersonName` | se conservan y se envuelven en `PersonIdentifier` |
| `TryReserveEmailAsync`, `ReleaseEmailAsync` | `TryReserveIdentifierAsync`, `ReleaseIdentifierAsync` |
| `UserRegistered`, `EmailChanged`, `EmailConfirmed`… | `PersonRegistered`, `IdentifierAdded`, `IdentifierVerified`… |
| `RegisterUser` | `RegisterPerson` (y `SignUp` para crear persona y cuenta a la vez) |
| `UpdateProfile` | `UpdatePersonProfile` + `AddIdentifier` / `ReplaceIdentifier` |
| `ConfirmEmail`, `ConfirmPhone` | `VerifyIdentifier` |
| `SetUserStatus` | se conserva, ahora sobre la cuenta (`User`) |
| `Application` | `Application.Write` |
| — | `User` (cuenta), `Advisor`, `ConsentLedger` y todo el lado de lectura: nuevos |

Como todavía no hay eventos guardados en ninguna base de datos, renombrar eventos y namespaces ahora no
tiene coste de migración.

## 10. Pendientes y decisiones abiertas

- **Envío de códigos a direcciones sin verificar.** Notification solo envía a las direcciones de su
  réplica `Recipient`, y esa réplica solo recibe direcciones verificadas. El código de verificación va
  justamente a una dirección que todavía no está verificada. Hay que resolverlo en Notification; las
  opciones que se barajaron fueron un campo de destino explícito en `NotificationDispatchRequested` o que
  Identity envíe por su cuenta.
- **Renombrar el contrato** `UserContactUpdated` a `PersonContactUpdated`. Afecta también a Notification.
- **¿Application.Read referencia a Domain o los eventos pasan a un proyecto propio?** Un proyecto
  `Domain.Events` haría que el compilador impida a la lectura cargar agregados.
- **¿Se puede anonimizar a alguien con publicaciones activas?** Requiere preguntar al servicio de
  Publicaciones.
- **Forma exacta de `PersonName` y `LegalName`**, y cómo lleva cada tipo `PersonNameChanged`.
- **AppHost:** añadir PostgreSQL para Marten y la API de Identity. Redis ya existe.
- **Al separar `Api.Read`:** decidir qué proceso ejecuta el daemon de Marten y las proyecciones inline, y
  configurar la réplica de lectura de PostgreSQL.

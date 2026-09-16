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
| **Identity** (este) | quién es cada persona, sus cuentas de acceso, sus perfiles de asesor y el inicio de sesión |
| **Publicaciones** (futuro) | inmuebles, publicación, favoritos, búsquedas, visitas y el resto de lógica del portal |
| **Notification** (existente) | envío de notificaciones por correo, SMS, push y WhatsApp |

**Cada servicio tiene su propia base de datos.** Identity usa la base `identity` en PostgreSQL; ningún
otro servicio la lee ni la escribe.

## 2. Responsabilidad del servicio

Identity responde a una sola pregunta, **"¿quién es?"**, en tres niveles:

| Nivel | Agregado | Responde |
|---|---|---|
| Identidad de los datos | `Person` | quién es esta persona y qué datos suyos están verificados |
| Identidad de acceso | `User` | con qué cuenta entra, qué roles tiene y si puede iniciar sesión |
| Identidad profesional | `Advisor` | si además es asesor y en qué condiciones trabaja |

Además guarda la prueba de los consentimientos de tratamiento de datos (`ConsentLedger`) y es el
**servidor de autorización** de la plataforma (OpenIddict): emite los tokens con los que la Web llama a
las APIs.

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
- En la base `identity` conviven dos almacenes, en esquemas separados: los flujos y documentos de Marten,
  y las tablas de OpenIddict (EF Core, esquema `oidc`).

### Por qué event sourcing en Identity

Se justifica por tres requisitos que en Identity no son accesorios:

1. **Auditoría de seguridad.** Quién bloqueó una cuenta, cuándo cambió la contraseña, quién otorgó el rol
   de administrador, quién revisó un documento. Con CRUD eso es una tabla de auditoría escrita *además*
   del estado, y puede quedar desincronizada. Con eventos, el historial **es** el estado: no se puede
   cambiar un `User` sin dejar `UserLocked`, `PasswordChanged` o `UserRolesChanged`.
2. **Prueba del consentimiento.** La Ley 1581 (art. 8) da derecho a solicitar prueba de la autorización:
   qué se autorizó, con qué versión de la política, desde qué origen, cuándo y cuándo se revocó. Un
   `UPDATE` borra justo lo que hay que probar; `ConsentLedger` es append-only por naturaleza.
3. **Fuente de las réplicas de otros servicios.** Rentals y Notification mantienen réplicas de Identity.
   Los eventos quedan guardados en la misma transacción que el cambio y una suscripción de Marten los
   publica sin perder ninguno. Un servicio nuevo reconstruye su réplica reproduciendo el historial, y una
   réplica dañada por un bug se repara reproduciéndolo otra vez.

Además:

- **Soporte y fraude necesitan la línea de tiempo.** Los duplicados se resuelven a mano desde soporte, y
  un robo de cuenta (cambio de teléfono → restablecer contraseña → cambio de canal preferido) solo se ve
  como secuencia.
- **Preguntas en el tiempo.** "¿Tenía el documento verificado cuando publicó?" sale de reproducir el
  flujo hasta esa fecha.
- **Coherencia del laboratorio.** Rentals ya usa event sourcing; Identity es el ejemplo de *cuándo*
  conviene el patrón, no solo de cómo se usa.

**Costes asumidos:**

| Coste | Cómo se resuelve |
|---|---|
| Los eventos viejos conservan datos personales | borrado criptográfico o enmascaramiento de Marten |
| La unicidad no es un índice único | reservas en el repositorio de escritura |
| Buscar por correo o listar usuarios | proyecciones |
| OpenIddict no vive en los flujos | dos almacenes; rotar el stamp y revocar sesiones no es atómico, pero `ValidateSession` rechaza el stamp viejo aunque la revocación falle |

**Alternativa descartada:** CRUD con EF Core, outbox de Wolverine y OpenIddict en el mismo `DbContext`.
Es más simple y hace atómica la revocación, pero obliga a reinventar un historial append-only para los
consentimientos y una auditoría paralela para la seguridad.

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
    `ContactPreferencesChanged`, `AdvisorProfileUpdated`, `UserRolesChanged`.

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
- En Marten, la reserva es un documento cuyo `Id` es el identificador normalizado y que guarda el
  `PersonId`; la clave primaria rechaza el duplicado. La misma reserva sirve para encontrar a la persona
  al iniciar sesión.
- "Una cuenta por persona" se resuelve igual: una reserva cuyo `Id` es el `PersonId` y que guarda el
  `UserId`.
- **Contrato:** las reservas y los eventos se guardan en la **misma sesión**, y
  `IUnitOfWork.SaveChangesAsync` los confirma juntos. Así no quedan reservas huérfanas si algo falla.

### Cuentas y roles

- `User` es la cuenta de acceso, con relación 1 a 1 con su `Person`.
- Se inicia sesión con **cualquier correo o teléfono verificado** de la persona. `User` no guarda el
  identificador.
- **Un login correcto no genera evento.** Solo quedan en el flujo los hechos de seguridad auditables:
  bloqueo, cambio de contraseña, cambio de roles, rotación del *security stamp*.
- **Los intentos fallidos tampoco generan evento.** Se cuentan fuera del agregado (Redis con TTL, puerto
  `ISignInAttemptCounter`); al superar el umbral, Application llama a `User.Lock`, que sí emite
  `UserLocked`. Así un atacante no puede inflar el flujo con intentos.
- **Security stamp:** valor que viaja dentro de la sesión (cookie de Identity, código y refresh token).
  Rotarlo (al cambiar o restablecer la contraseña, bloquear o deshabilitar la cuenta) invalida todas las
  sesiones abiertas.
- El hash de contraseña lo calcula `IPasswordHasher`, en Application. El agregado recibe el
  `PasswordHash` ya calculado y la política de contraseñas es `PasswordPolicy`, en Domain.
- **Roles:** por ahora solo `Administrador`, guardado en `User` y emitido como claim `role`.
  **"Propietario" no es un rol:** sale del dato (`Listing.PublisherId == person_id`), porque cualquier
  usuario puede publicar y buscar a la vez, y un rol en el token quedaría desactualizado al publicar el
  primer inmueble. Quien busca tampoco es un rol; en `Rentals.md` se llama **Interesado**.
- Cambiar los roles no rota el stamp: el siguiente refresh ya emite el token con los roles nuevos.

### Autenticación con OpenIddict

**Qué es cada pieza (equivalencias con IdentityServer4):**

| IdentityServer4 | OpenIddict |
|---|---|
| `Client` | *Application* (`IOpenIddictApplicationManager`) |
| `ApiScope` / `ApiResource` | *Scope* con *Resources* (`IOpenIddictScopeManager`) |
| `PersistedGrant` | *Token* + *Authorization* |
| `IProfileService` | los claims se arman en los endpoints *passthrough* |
| Quickstart UI | páginas propias en `IdentityService.Api` |
| `AddDeveloperSigningCredential` | `AddDevelopmentSigningCertificate()` |
| `IdentityServer4.AccessTokenValidation` | `OpenIddict.Validation.AspNetCore` |

- **Passthrough:** OpenIddict valida que la petición cumpla el protocolo y la entrega a nuestro endpoint,
  que decide quién es el usuario y llama a `SignIn`. No se usa ASP.NET Core Identity.
- **Destinations:** un claim solo llega al access token o al id_token si se le asigna destino. Los claims
  sin destino viajan en el código y en el refresh token, que solo lee el servidor; así va el
  `security_stamp`.

**Qué es stateless y qué no:**

| Pieza | Stateless | Dónde vive |
|---|---|---|
| Access token (JWT firmado, sin cifrar, 5 a 15 min) | sí | lo validan las APIs con la clave pública |
| Cookie de sesión de Identity (sesión SSO) | sí | navegador, cifrada |
| Cookie de la Web | sí | navegador, cifrada; guarda los tokens |
| Código de autorización (un solo uso) | no | store de OpenIddict |
| Refresh token (revocable, rotativo) | no | store de OpenIddict |
| Clientes y scopes | no | store de OpenIddict |

Por eso **las sesiones no van en Redis**: una sesión es una *authorization* de OpenIddict con su refresh
token, y OpenIddict solo trae stores para EF Core y MongoDB. Se usa EF Core en la base `identity`.

**Flujo para la Web: Authorization Code + PKCE, cliente confidencial.** No se usa el grant de contraseña.

```
Web ──challenge──► Identity /connect/authorize
                     ¿cookie de Identity? no ──► /account/login
                     POST login → SignIn (contraseña, intentos, estado)
                               → cookie de Identity con sub, person_id, roles y security_stamp
                     /connect/authorize → SignIn(OpenIddict) → código
Web /signin-oidc ──código──► /connect/token → access, id y refresh token
Web → APIs con Authorization: Bearer <access_token>
```

- **La página de login vive en Identity**: las credenciales solo las ve el servidor de autorización.
- **Refresh:** `/connect/token` llama a `ValidateSession`. Si la cuenta no puede entrar o el stamp
  cambió, responde `invalid_grant`; si no, emite tokens con los datos y roles actuales.
- **Cerrar sesión:** la Web borra su cookie y redirige a `/connect/endsession`, que borra la cookie de
  Identity. Los access tokens emitidos siguen valiendo hasta vencer.
- **Revocación:** bloquear, deshabilitar o cambiar la contraseña rota el stamp y revoca las autorizaciones
  del usuario (`ISessionRevoker`). "Mis sesiones" lista esas autorizaciones y permite cerrar una.

**Claims del token:**

| Claim | Destino | Uso |
|---|---|---|
| `sub` | access, id | `UserId`; el actor en todas las APIs |
| `person_id` | access | `PublisherId` en Rentals |
| `name` | access, id | mostrar en la UI |
| `role` | access, id | `administrador` |
| `security_stamp` | ninguno | se compara en cada refresh |

El permiso de asesor y el nivel de verificación **no** se usan desde el token para decidir: Rentals los
toma de su réplica, que está más al día que un token emitido hace minutos.

### Autorización en los servicios

Tres capas, cada una en su sitio:

1. **Autenticación, en la Api.** Validación local con `OpenIddict.Validation.AspNetCore`: issuer y claves
   por discovery, audience propio por API. Sin introspección, para no llamar a Identity en cada petición.
2. **Permisos gruesos, en los endpoints.** Scope de la API y rol `administrador` donde aplique
   (administración de Notification, moderación en Rentals, administración de usuarios en Identity).
3. **Autorización por dato, en Application.** Los endpoints toman el actor de `sub` y lo pasan al
   comando; los contratos HTTP ya no llevan `ActorId`. Los handlers siguen comparando el actor con el
   dueño del dato. Las consultas "mías" usan `/api/me/...`, nunca un id en la ruta.

La mensajería no lleva tokens de usuario: los consumidores confían en el broker. Una llamada HTTP entre
servicios sin usuario usaría el flujo *client credentials*.

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
- **Application no conoce OpenIddict.** Solo expone puertos (`ISessionRevoker`) y devuelve
  `SignedInUser`; la Api lo convierte en cookie o en token.
- **CQRS con proyectos separados de escritura y lectura:**
  - **Regla:** si el dato sirve para decidir, va por escritura, contra el flujo y con consistencia
    fuerte (el login, las reservas, `ValidateSession`). Si sirve para mostrarse, va por lectura.
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
- La carpeta de OpenIddict se llama `Oidc/`, no `OpenIddict/`, para que el namespace no tape al del
  paquete.

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
- Solo se inicia sesión con un correo o teléfono **verificado** y con la persona `Active`
  (`CanSignInWith`).
- Si la persona tiene una cuenta activa, debe conservar al menos un correo o teléfono verificado, porque
  es con lo que inicia sesión. Lo comprueba Application antes de quitar un identificador.
- Una persona anonimizada queda en estado final y rechaza cualquier comando.

### 5.2 `User`

**Datos:** `PersonId`, estado, hash de contraseña, security stamp, roles y, si está bloqueada, hasta
cuándo.

**Estados:** `Active | Locked | Disabled`.

**Eventos:** `UserCreated(UserId, PersonId)`, `PasswordChanged`, `UserLocked(Reason, Until)`,
`UserUnlocked`, `UserDisabled`, `UserEnabled`, `SecurityStampRotated`, `UserRolesChanged(Roles)`.

**Reglas:**
- Cada persona tiene como máximo una cuenta (reserva por `PersonId`).
- Para crear la cuenta, la persona necesita al menos un correo o teléfono verificado.
- Varios intentos fallidos seguidos generan `UserLocked`. Ni un login correcto ni un intento fallido
  generan evento.
- Un bloqueo con fecha vence solo: `CanSignIn(now)` lo da por terminado sin necesidad de `UserUnlocked`.
- Cambiar o restablecer la contraseña, bloquear o deshabilitar la cuenta rota el security stamp.
- Una cuenta bloqueada o deshabilitada no puede iniciar sesión ni refrescar su sesión.
- Cambiar los roles con el mismo conjunto no genera evento.

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

- **Publicación de eventos:** una suscripción de Marten (`IntegrationEventsSubscription`, el mismo patrón
  que `DomainEventsSubscription` en Rentals) lee los eventos guardados, arma el contrato con el estado
  completo y lo publica con Wolverine. No hace falta outbox ni puerto de publicación en Application.
- **Topología de RabbitMQ:** Identity publica a **exchanges** (`identity.users`, `identity.contacts`, …) y
  cada servicio enlaza **su propia cola**. Con una cola compartida, el mensaje lo recibiría un solo
  servicio.
- **La réplica se une al token por el id:** el `sub` del token es la clave de la réplica local. Ningún
  servicio consulta a Identity de forma síncrona.
- **Consumidores:** idempotentes (ignoran mensajes más viejos que la réplica) y preparados para recibir
  una petición con token válido antes de que llegue el mensaje de la cuenta.
- **Notification:**
  - **Cambios de contacto:** contrato `PersonContactUpdated` (hoy `UserContactUpdated`). Solo se envían
    direcciones **verificadas**; una cadena vacía significa "no hay valor verificado".
  - **Códigos de verificación:** se envían con `IVerificationCodeSender`.
  - **API:** valida el token y exige el rol `administrador`.
- **Rentals:**
  - Guarda referencias por `PersonId`, `UserId` y `AdvisorId`.
  - Consume `UserAccountChanged`, `PersonVerificationChanged`, `AdvisorChanged` y
    `AlertsConsentChanged`.
  - Toma el actor del claim `sub`; la moderación exige el rol `administrador`.
- **Web:** cliente OIDC confidencial (cookie + `OpenIdConnect`), envía el access token a las APIs y
  administra usuarios contra `IdentityService.Api`.

**Contratos que publica Identity:**

| Contrato | Contenido | Consumidores |
|---|---|---|
| `UserAccountChanged` | user_id, person_id, nombre, estado, roles | Rentals |
| `PersonContactUpdated` | direcciones verificadas y preferencias | Notification |
| `PersonVerificationChanged` | teléfono y documento verificados | Rentals |
| `AdvisorChanged` | estado, capacidad y zonas | Rentals |
| `AlertsConsentChanged` | consentimiento de alertas | Rentals |

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

Shared/
├── Domain.Shared
├── Application.Shared
├── NotificationLog.Contracts              + contratos de Identity
└── NotificationLog.ServiceDefaults        + validación de tokens compartida
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

**Dónde queda OpenIddict:**

| Proyecto | Paquete | Qué hace ahí |
|---|---|---|
| Domain | ninguno | cuenta, contraseña, estado, roles y security stamp |
| Application.Write / Read | ninguno | casos de uso y puertos |
| Infrastructure.Write | `OpenIddict.EntityFrameworkCore` | stores, managers, `DbContext`, seed de clientes, revocación |
| Infrastructure.Read | `OpenIddict.Core` | listar las sesiones de un usuario |
| Api | `OpenIddict.Server.AspNetCore` | `/connect/*`, certificados, cookie de Identity, páginas de login |
| ServiceDefaults | `OpenIddict.Validation.AspNetCore`, `OpenIddict.Validation.SystemNetHttp` | validar tokens en cada API |
| Web | `Microsoft.AspNetCore.Authentication.OpenIdConnect` | cliente OIDC |

**Core** es persistencia (Infrastructure), **Server** es protocolo HTTP (Api) y **Validation** es
transversal a las APIs (ServiceDefaults).

## 8. Estructura de archivos

### 8.1 Domain

```
NotificationLog.IdentityService.Domain/
├── Persons/
│   ├── Person.cs
│   ├── IPersonRepository.cs               Load, Append, TryReserveIdentifier, ReleaseIdentifier, FindIdByIdentifier
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
│   ├── IUserRepository.cs                 Load, Append, TryReserveAccount, FindIdByPersonId
│   ├── UserStatus.cs                      Active | Locked | Disabled
│   ├── UserRole.cs                        Administrador
│   ├── PasswordPolicy.cs
│   ├── Events/
│   │   ├── UserCreated.cs
│   │   ├── PasswordChanged.cs
│   │   ├── UserLocked.cs
│   │   ├── UserUnlocked.cs
│   │   ├── UserDisabled.cs
│   │   ├── UserEnabled.cs
│   │   ├── SecurityStampRotated.cs
│   │   └── UserRolesChanged.cs
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
│   ├── ISignInAttemptCounter.cs           intentos fallidos con TTL
│   └── ISessionRevoker.cs                 RevokeAll(userId), Revoke(userId, sessionId)
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
│   └── AnonymizePerson/ *                 también deshabilita el User, retira el Advisor y revoca sesiones
├── Users/
│   ├── SignedInUser.cs                    UserId, PersonId, Name, Roles, SecurityStamp
│   ├── Commands/
│   │   ├── SignUp/                        crea Person y User; devuelve SignedInUser
│   │   ├── CreateUser/                    cuenta para una Person que ya existía
│   │   ├── SignIn/                        persona → cuenta → hash → intentos/bloqueo; devuelve SignedInUser
│   │   ├── RevokeSession/ *               cerrar una sesión desde "Mis sesiones"
│   │   ├── ChangePassword/                rota el stamp y revoca sesiones
│   │   ├── RequestPasswordReset/
│   │   ├── ResetPassword/                 rota el stamp y revoca sesiones
│   │   ├── SetUserStatus/ *               al bloquear o deshabilitar rota el stamp y revoca sesiones
│   │   ├── UnlockUser/ *
│   │   └── SetUserRoles/
│   └── Queries/
│       └── ValidateSession/               en escritura porque decide: SignedInUser actualizado o null
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
│   ├── IAccountQueries.cs                 mi cuenta y mis sesiones
│   ├── IUserQueries.cs                    listado para administración
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
│   │   ├── ListMySessions/
│   │   └── SearchUsers/
│   ├── Dtos/
│   │   ├── AccountDto.cs
│   │   ├── SessionDto.cs                  SessionId (id de la autorización), cliente, fecha
│   │   └── UserSummaryDto.cs
│   └── Projections/
│       ├── UserView.cs
│       └── UserViewProjector.cs
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
│       ├── IdentifierReservation.cs       Id = identificador normalizado, guarda el PersonId
│       └── AccountReservation.cs          Id = PersonId, guarda el UserId
├── Oidc/
│   ├── OidcDbContext.cs                   UseOpenIddict(), esquema "oidc"
│   ├── OidcDbContextFactory.cs            design-time
│   ├── Migrations/
│   ├── OidcClientOptions.cs               ClientId, secreto y redirect URIs de la Web
│   ├── OidcSeeder.cs                      IHostedService: cliente "web" y scopes con sus resources
│   └── OpenIddictSessionRevoker.cs        ISessionRevoker con IOpenIddictAuthorizationManager
├── Security/
│   └── IdentityPasswordHasher.cs
├── Sessions/
│   └── RedisSignInAttemptCounter.cs
├── Verification/
│   ├── RedisVerificationCodeStore.cs
│   └── NotificationVerificationCodeSender.cs
├── Messaging/
│   ├── RabbitMqMessagingExtensions.cs     exchanges identity.users, identity.contacts, …
│   ├── Subscriptions/
│   │   └── IntegrationEventsSubscription.cs
│   └── Mapping/
│       ├── UserAccountChangedMapper.cs
│       ├── PersonContactUpdatedMapper.cs
│       ├── PersonVerificationChangedMapper.cs
│       ├── AdvisorChangedMapper.cs
│       └── AlertsConsentChangedMapper.cs
└── DependencyInjection.cs                 AddIdentityWrite(): Marten, OidcDbContext, AddOpenIddict().AddCore()
```

### 8.5 Infrastructure.Read

```
NotificationLog.IdentityService.Infrastructure.Read/
├── Projections/
│   ├── PersonViewProjection.cs            SingleStreamProjection que delega en PersonViewProjector
│   ├── UserViewProjection.cs              SingleStreamProjection que delega en UserViewProjector
│   └── AdvisorViewProjection.cs           SingleStreamProjection que delega en AdvisorViewProjector
├── Queries/
│   ├── MartenPersonQueries.cs
│   ├── MartenAccountQueries.cs            vista de Marten + autorizaciones de OpenIddict
│   ├── MartenUserQueries.cs
│   └── MartenAdvisorQueries.cs
└── DependencyInjection.cs                 AddIdentityRead()
```

### 8.6 Api

```
NotificationLog.IdentityService.Api/
├── Program.cs                             AddIdentityWrite(), AddIdentityRead(), AddIdentityOidcServer(),
│                                          AddTokenValidation(identity-api), AddRazorPages, MapOidc()
├── appsettings.json
├── Properties/launchSettings.json         puerto fijo: es el issuer
├── Oidc/
│   ├── OidcServerExtensions.cs            cookie de Identity + AddOpenIddict().AddServer()
│   ├── OidcEndpoints.cs                   /connect/authorize, /connect/token, /connect/endsession
│   └── OidcPrincipalFactory.cs            SignedInUser → principal de la cookie o del token
├── Pages/
│   ├── _ViewImports.cshtml
│   ├── Shared/_Layout.cshtml
│   └── Account/
│       ├── Login.cshtml
│       ├── Login.cshtml.cs                SignIn → cookie → returnUrl
│       ├── Register.cshtml
│       └── Register.cshtml.cs             SignUp → cookie → returnUrl
├── Endpoints/
│   ├── PersonEndpoints.cs
│   ├── AccountEndpoints.cs                mi cuenta, mis sesiones, cambiar contraseña
│   ├── UserEndpoints.cs                   administración: buscar, estado, roles, desbloquear
│   ├── AdvisorEndpoints.cs
│   └── ConsentEndpoints.cs
├── Contracts/
│   ├── Persons/                           un request/response por endpoint, como en Notification
│   ├── Accounts/
│   ├── Users/
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
    ├── Users/                             SignIn, ValidateSession, SetUserRoles
    └── Advisors/
```

### 8.8 Fuera de Identity

Leyenda: `+` nuevo · `~` cambia · `-` se elimina.

```
NotificationLog.AppHost/
└── ~ AppHost.cs                           base "identity" en postgres, proyecto Identity con puerto fijo y
                                           WithExternalHttpEndpoints, parámetro secreto del cliente web,
                                           WithReference(identity) en web, rentals y apiservice

Shared/NotificationLog.ServiceDefaults/
└── + Authentication/
    ├── + TokenValidationExtensions.cs     AddTokenValidation(audience)
    ├── + ClaimsPrincipalExtensions.cs     GetUserId(), GetPersonId()
    ├── + PlatformClaims.cs
    ├── + PlatformRoles.cs
    ├── + PlatformScopes.cs
    └── + PlatformAudiences.cs

Shared/NotificationLog.Contracts/
├── Identity/
│   ├── + user_account_changed.proto
│   └── + person_contact_updated.proto
└── Users/
    └── - user_contact_updated.proto

Services/Rentals/NotificationLog.RentalService.Infrastructure/
├── IdentityReplica/
│   ├── + UserAccountDocument.cs
│   └── ~ MartenIdentityReplica.cs
└── Messaging/
    ├── ~ RabbitMqMessagingExtensions.cs   colas enlazadas a los exchanges de Identity
    └── Consumers/
        └── + UserAccountChangedHandler.cs

Services/Rentals/NotificationLog.RentalService.Api/
├── ~ Program.cs                           AddTokenValidation(rentals-api)
├── Endpoints/
│   ├── ~ ListingEndpoints.cs, OfferEndpoints.cs, VisitEndpoints.cs, InquiryEndpoints.cs
│   │                                      actor desde sub; lectura pública con AllowAnonymous
│   ├── ~ FavoriteEndpoints.cs, SavedSearchEndpoints.cs    /api/users/{userId}/… → /api/me/…
│   ├── ~ ModerationEndpoints.cs           rol administrador
│   └── - DevIdentityEndpoints.cs
└── Contracts/
    ├── ~ Listings/, Moderation/, Offers/, Visits/, Inquiries/    sin ActorId ni ModeratorId
    ├── - SubmitListingForReviewRequest.cs, RenewListingRequest.cs, ExtendReservationRequest.cs,
    │     ReinstateListingRequest.cs, AcceptOfferRequest.cs, WithdrawOfferRequest.cs   solo tenían el actor
    └── - Dev/

Services/Notification/
├── NotificationLog.NotificationService.Infrastructure/Messaging/
│   ├── RabbitMq/~ RabbitMqMessagingExtensions.cs    cola enlazada a identity.contacts
│   └── Consumers/~ UserContactUpdatedHandler.cs     → PersonContactUpdatedHandler.cs
└── NotificationLog.ApiService/
    ├── ~ Program.cs                       AddTokenValidation(notification-api)
    └── Endpoints/~ *.cs                   rol administrador en cada grupo

NotificationLog.Web/
├── ~ Program.cs                           autenticación, AccessTokenHandler en cada HttpClient, circuit handler
├── + Authentication/
│   ├── + WebAuthenticationExtensions.cs   cookie + OpenIdConnect (code, PKCE, SaveTokens, scopes)
│   ├── + CookieOidcRefresher.cs
│   ├── + AccessTokenHandler.cs
│   ├── + CircuitServicesAccessor.cs
│   ├── + ServicesAccessorCircuitHandler.cs
│   └── + LoginLogoutEndpoints.cs
├── Api/
│   ├── + Identity/  AccountApiClient.cs, AccountModels.cs, UsersApiClient.cs, UserModels.cs
│   └── Rentals/
│       ├── - Identity/
│       └── ~ */*Models.cs, ModerationApiClient.cs, UserCollectionsApiClient.cs    sin actor, rutas /api/me
└── Components/
    ├── ~ Routes.razor                     AuthorizeRouteView
    ├── ~ _Imports.razor
    ├── Layout/
    │   ├── + LoginDisplay.razor
    │   ├── ~ MainLayout.razor, NavMenu.razor, RentalsLayout.razor
    ├── Rentals/
    │   ├── ~ RentalsActor.cs              lee el usuario autenticado
    │   └── - ActorBar.razor
    └── Pages/
        ├── + Account/MySessions.razor
        ├── + Admin/Users/UsersList.razor, UserDetail.razor
        ├── - Rentals/Identity/IdentityReplica.razor
        └── ~ Rentals/Moderation/, Templates/, Triggers/, Recipients/, Notifications/   rol administrador
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

**Lo que hoy suple la autenticación:**
- La Web elige con quién actuar en la barra «Actuando como» (`RentalsActor`, `ActorBar`) y envía
  `ActorId` o `ModeratorId` en el body.
- Rentals simula los eventos de Identity con `DevIdentityEndpoints`.
- Ninguna API valida tokens, y nadie comprueba el permiso de moderador.

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
| «Actuando como» y `ActorId` en el body | usuario autenticado y claim `sub` |
| `DevIdentityEndpoints` | eventos reales publicados por Identity |
| — | `User` (cuenta), `Advisor`, `ConsentLedger`, OpenIddict y todo el lado de lectura: nuevos |

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
- **AppHost:** añadir la base `identity` en PostgreSQL y la API de Identity. Redis ya existe.
- **Al separar `Api.Read`:** decidir qué proceso ejecuta el daemon de Marten y las proyecciones inline, y
  configurar la réplica de lectura de PostgreSQL.
- **Roles:** confirmar que "propietario" se deriva del dato y que quien busca se llama **Interesado**.
  Si más adelante se separa `Moderador` de `Administrador`, cambian `UserRole`, `PlatformRoles`,
  `ModerationEndpoints` y la página de moderación.
- **Validación de tokens en `ServiceDefaults`** o en un proyecto `Shared/NotificationLog.Authentication`.
  ServiceDefaults evita un proyecto más, pero la Web carga el paquete de validación sin usarlo.
- **403 para "no es tuyo":** hoy los handlers de Rentals lanzan `AppValidationException` (400). Una
  `ForbiddenException` en `Application.Shared` con su caso en los `GlobalExceptionHandler` lo haría 403.
- **«Actuando como»:** eliminarlo o conservarlo solo en Development para los tutoriales.
- **Tokens en Blazor Server:** el `AccessTokenHandler` no tiene `HttpContext` dentro del circuito; seguir
  la muestra oficial `BlazorWebAppOidcServer` para leer y refrescar el token.

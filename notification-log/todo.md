# Pendientes

## Prioridad alta

- [x] **Token hacia las APIs.** `AccessTokenHandler` agrega `Authorization: Bearer` en los tres clientes y `CookieOidcRefresher` renueva el token con el refresh token al validar la cookie (patrón `BlazorWebAppOidcServer`).
  - [ ] La renovación solo ocurre en una petición HTTP. Con `InteractiveServer` global, un circuito abierto más de 15 min (vida del access token) sin recargar la página sigue enviando el token vencido y las APIs responden 401.
- [ ] **Validar tokens en Rentals y Notification.**
  - [x] OpenIddict Validation con su audience en las dos APIs.
  - [x] Toda la API de Notification (triggers, plantillas, destinatarios y envíos) solo para el administrador (scope `notifications` + `AdministradorPolicy`).
  - Policies por rol.
  - Actor desde `sub` en lugar de `ActorId` en el body.
  - Rutas `/api/me/...` en lugar de `/api/users/{userId}/...`.

## Identity

- [ ] UI de administración de usuarios en la Web: listado, roles, bloquear y desbloquear.
- [ ] Agregar o cambiar el correo o el celular de una cuenta existente, con código y evento `UserContactChanged`.
- [ ] **Propagar los cambios de perfil.** `UserCreated` es el único evento que lleva el perfil completo y `RecipientSyncService` lo ignora si el destinatario ya existe, así que actualizar nombre, idioma o zona horaria (por ejemplo con un mensaje a la cola `identity-accounts` de una cuenta que ya está) no llega a Notification. Falta un evento de actualización de perfil, o que el consumidor haga upsert.
- [ ] Recuperar y cambiar la contraseña.
- [ ] Páginas de error del protocolo, acceso denegado y confirmación de cierre de sesión.
- [ ] Apariencia por cliente en el login, según el `client_id` validado.
- [ ] Scalar con OAuth para probar `/api/users` sin la Web.
- [ ] **Rol según dónde se registre la cuenta.** Quien se registra en la plataforma pública (solo quiere ver contenido) queda como `Cliente`; quien se registra desde el backoffice queda como `Propietario`. Después solo un administrador puede cambiarle el rol. Hace falta el rol `Cliente`: el enum `UserRole` hoy solo tiene `Administrador`, `Propietario` y `Moderador`.

## Web

- [ ] **Compartir la hoja de estilos de "Llave".** Los mismos tokens de marca viven duplicados en `IdentityService.Api/wwwroot/css/llave.css` (login y registro), en `Web/wwwroot/app.css` y en `UI/portal/app/globals.css`; unificarlos para que un cambio de marca se haga en un solo sitio.
- [ ] Página propia de acceso denegado: `/authentication/access-denied` todavía responde texto plano.
- [ ] **Impedir que un visitante inicie sesión en el backoffice.** El rol `Visitor` solo usa el portal; hoy puede autenticarse en la Web con el cliente `web` y entrar a las páginas que no exigen rol (Panel, Publicaciones, Visitas, Notificaciones, Mi perfil).
- [ ] **Título del inmueble dentro de `VisitView`.** `VisitViewProjection` copia tipo, barrio y ciudad de la publicación cuando se pide la visita; si después llega un `ListingDetailsUpdated`, las visitas ya existentes conservan el título anterior. Propagarlo a las visitas de esa publicación.
- [ ] **Nombre de quien creó la publicación.** `ListingView.CreatedBy` es un id de usuario de Identity y Rentals no conoce nombres de usuarios; la Web muestra «Tú», el propietario o el id corto.

## Portal (Next.js)

- [ ] Primera foto en `ListingSummaryDto` para que las tarjetas del catálogo muestren imagen en lugar del logo.
- [ ] Renovar el access token con refresh token; hoy, cuando vence (1 h), la sesión del portal termina y hay que volver a iniciar sesión (Identity la recuerda, así que es solo una redirección).
- [ ] El catálogo público devuelve `ListingDto` completo (incluye dirección y `OwnerId`); decidir qué datos se muestran sin iniciar sesión.

## Integración y negocio

- [ ] **Log de eventos reproducible** (estilo Kafka) para reconstruir estado o alimentar servicios nuevos; hoy RabbitMQ solo garantiza la entrega.
- [ ] **Servicio de Personas** (con capas): `Person` con o sin cuenta, consentimientos y verificación de documento. Reemplaza la réplica dev de `/rentals/identity`.
- [ ] Responder 403 en lugar de 400 para "no es tuyo" en las visitas de Rentals (`ForbiddenException`); publicaciones y propietarios ya lo hacen.
- [ ] **Recordatorios programados en Notification** (reemplaza `WarnListingExpiryCommand` y `WarnListingExpiryHandler`; el porqué está en `EventSourcingProblems.md`, caso 1).
  - **Idea.** Un agregado nuevo en Notification (`Reminder`, o `ScheduledReminder`) que funciona como `Notification`, pero diferido: se registra ahora y se envía después. El productor no dice cómo ni cuándo enviar; eso ya está configurado en Notification. Rentals solo manda tres cosas: la **clave** de la configuración (por ejemplo `publicacion.vence.pronto`), el **id del flujo** que origina el recordatorio (el `ListingId`) y el **payload** con los valores que reemplazan los placeholders de la plantilla.
  - **Configuración** (lado de Notification, como `NotificationTrigger`).
    - `EventKey` como identidad, con el mismo formato y regex.
    - Canales y plantillas: se reutilizan `NotificationConfiguration` y `NotificationTemplate`, así que el render (Scriban, `StrictVariables`) y el envío son los de hoy.
    - El momento del envío: un desfase respecto a una fecha del payload, por ejemplo "7 días antes de `expires_at`". Así la regla de negocio de cuánto antes avisar (RN-08) queda en la configuración y Rentals no programa nada.
    - Decidir si el desfase también puede ser absoluto ("en X horas desde que llega") para otros casos, como los recordatorios de visita de 24 h y 2 h.
  - **Identidad del recordatorio.** El par `(clave, id del flujo)`. Recibir otra vez el mismo par **reemplaza** el recordatorio pendiente en lugar de crear otro. Eso resuelve la renovación: `ListingRenewed` manda el nuevo `expires_at` y el aviso viejo desaparece, sin chequear si está obsoleto al ejecutarse.
  - **Estados.** `Pending → Sent | Failed | Cancelled`. Al enviarse deja un `Notification` en el log, igual que hoy (éxito o fallo, nunca se descarta en silencio). Enviado o cancelado, ya no se reemplaza.
  - **Cancelación.** Un mensaje aparte con `(clave, id del flujo)`. Rentals lo manda cuando la publicación deja de estar vigente: `ListingClosed`, `ListingWithdrawn` y `ListingSuspended`. Cancelar algo que no existe o que ya se envió no es un error.
  - **Rehabilitación.** `ListingReinstated` debe volver a programar el aviso con el `ExpiresAt` vigente (el evento no lo trae, hay que leerlo del `Listing`). Si ya pasó la fecha del aviso, se envía de inmediato o se omite: decidir.
  - **Pausa.** El handler actual sí avisa a las publicaciones pausadas. Mantenerlo: pausar no cancela el recordatorio.
  - **Destinatario.** Falta en los tres datos de la idea. `NotificationDispatchRequested` lleva `recipient_id`; el recordatorio también lo necesita (el anfitrión, `Listing.HostOf`). Mandarlo como campo propio del contrato, no dentro del payload.
  - **Envío.** Un proceso en Notification que cada minuto toma los `Pending` con `DueAt <= now` (índice por `Status, DueAt`) y los pasa por el mismo camino de `NotificationDispatchService`. Aquí la consulta por rango de fechas es trivial porque Notification es relacional (EF Core + SQL Server); ese es justamente el motivo de mover el recordatorio a este servicio. Controlar el envío doble si hay varias instancias (`UPDATE ... WHERE Status = Pending` como bloqueo optimista, o `rowversion`).
  - **Recalcular.** Si el payload cambia pero la fecha no, se reemplaza igual (el render usa el último payload). La fecha de envío se recalcula cada vez que llega el mensaje, no cuando cambia la configuración: decidir si cambiar el desfase en la configuración mueve los pendientes.
  - **Contratos.**
    - `reminder_scheduled.proto` (`event_id`, `occurred_at`, `schema_version`, `reminder_key`, `stream_id`, `recipient_id`, `map<string,string> data`) y `reminder_cancelled.proto` (`reminder_key`, `stream_id`) en `Shared/NotificationLog.Contracts/Notifications`, con los comentarios de semántica como los otros `.proto`. El primero es snapshot completo, igual que `UserContactUpdated`.
    - Cola `notification-reminders` en `RabbitMqMessagingExtensions`, con consumidores estáticos que solo mapean a records de Application.
  - **Rentals.**
    - `IReminderProducer` en `Common/Producers`, al lado de `INotificationProducer`.
    - `ListingNotificationsProcess` programa en `ListingApproved`, `ListingRenewed` y `ListingReinstated`, y cancela en `ListingClosed`, `ListingWithdrawn` y `ListingSuspended`.
    - `ListingLifecycleProcess` queda solo con `ExpireListingCommand`, que sí cambia el estado y sigue siendo un mensaje programado de Wolverine.
    - Actualizar la fila de `ListingApproved` / `ListingRenewed` en la tabla de `Rentals.md` §6.
  - **API y Web.** Endpoints `MapReminders()` para administrar la configuración y consultar los pendientes, y página en la Web con su `ReminderApiClient`. Plantilla y configuración semilla para `publicacion.vence.pronto` con `{{ expires_at }}` y `{{ listing_id }}`.
  - **Tests.** Reemplazo por `(clave, id del flujo)`, cancelación, idempotencia ante mensajes repetidos, y que un placeholder sin valor en el payload quede como `Notification` fallida.

## Rentals

- [ ] **Schedules para tareas programadas** (vencer una publicación, avisar que está por vencer, recordatorios de visita). Reemplaza el `ApplyExpiry` calculado en `Listing.Status` y en `MartenListingReadModel`, y también `ICommandScheduler`, `WolverineCommandScheduler`, `ScheduledCommandsHandler` y `ExpireListingCommand`.
  - **Datos del dominio.** Por ahora solo `StreamId`, `ScheduledTo` (fecha y hora) y `Kind` (string que identifica cómo se procesa, por ejemplo `listing.expire` o `listing.warn-expiry`). El agregado declara sus schedules a partir de su estado; `Listing` los deriva de `IsVisible` y `ExpiresAt`.
  - **Programar.** Mensaje programado de Wolverine por el outbox (`IMartenOutbox` enrolado en la misma sesión de Marten), para que sea atómico con los eventos. La dependencia queda invertida: la interfaz en Application y la implementación en Infrastructure. `AggregateStreams` compara los schedules del agregado al cargarlo y al hacer append, y programa solo los nuevos.
  - **Validar al ejecutarse.** Recargar el agregado y seguir solo si el schedule `(Kind, ScheduledTo)` todavía está entre los que declara; si no, descartarlo. Los mensajes programados no se cancelan, así que renovar, cerrar, retirar o suspender dejan mensajes viejos que se descartan al llegar.
  - **Duplicados.** Suspender y rehabilitar con el mismo `ExpiresAt` vuelve a programar el aviso y el original sigue vivo. Expirar es idempotente en el dominio; para avisar, usar un `event_id` determinista (`StreamId + Kind + ScheduledTo`) y que Notification ignore los repetidos.
  - **Estructura en Application (por definir).** Nombre de la carpeta y de las clases que reciben el schedule vencido, lo enrutan por `Kind` al handler que corresponde y disparan comandos o notificaciones. Es parecido a un consumer, pero ese nombre no encaja para esta lógica. También hay que decidir si hay un handler por `Kind` o uno por agregado.
- [ ] **Visitas: tareas programadas, cancelación automática y reglas sin validar.** El dominio (`Visit`) y los comandos de usuario ya existen; falta lo que no dispara una persona.
  - **Vencer la visita sin respuesta.** Al llegar el `RespondBy` de cada propuesta, llamar a `visit.Expire(now)` (emite `VisitExpired`). El método ya existe; faltan el comando `ExpireVisit` y programarlo tras `VisitRequested` y `VisitCounterProposed`. Avisar a las dos partes (`visita.vencida`).
  - **Cierre automático.** 72 h después de que termine la franja agendada, llamar a `visit.AutoComplete(now)` (emite `VisitCompleted` con `System`). El método ya existe; faltan el comando y programarlo tras `VisitScheduled`.
  - **Recordatorios de la visita agendada** a las dos partes (por ejemplo, 24 h antes). Solo notifican, no cambian el estado (`visita.recordatorio`).
  - Los dos métodos del dominio no hacen nada si todavía no toca o la visita ya cambió de estado, así que un mensaje viejo o repetido es inofensivo. Encaja con el pendiente de *Schedules* de arriba.
  - **Cancelar las visitas cuando la publicación deja de estar disponible** (cerrada, retirada, suspendida o vencida), con `visit.Cancel(VisitParty.System, ...)`, y avisar a las dos partes. Necesita una forma de encontrar las visitas activas de una publicación (proyección por `ListingId`).
  - **Reglas que no se validan todavía** (todas consultan varias visitas a la vez, contra una proyección):
    - máximo de visitas activas por visitante;
    - una sola visita activa por visitante y publicación (hoy se puede pedir dos veces la misma);
    - bloqueo por cancelaciones tardías o inasistencias repetidas (`IsLateCancellation`, `NoShow`);
    - que el anfitrión no agende dos visitas a la misma hora.
- [ ] Visitas con el usuario del token en lugar de `ActorId` en el body, como ya hacen publicaciones y propietarios.
- [ ] **Quitar la réplica de Identity (`IIdentityReplica`) y cambiar antes la regla de visitas.**
  - **Qué es hoy.** `IIdentityReplica` **no** consulta la base de Identity: lee la base de *Rentals* (Marten,
    esquema `rentals`), el documento `PersonVerificationDocument`. Es una copia local de un dato ajeno.
  - **Cómo se llena.** Identity publica `PersonVerificationChanged` al exchange fanout `identity.users` cuando
    alguien confirma su correo o su teléfono; Rentals lo escucha en la cola `rentals-person-verification` y
    `PersonVerificationChangedHandler` hace upsert del documento. `MartenIdentityReplica` después lo lee por `UserId`.
  - **Para qué sirve.** Para una sola regla: `ListingAccess.RequireVerifiedUserAsync`, que solo usa
    `RequestVisitHandler` (nadie pide una visita sin teléfono verificado). La idea era no llamar a Identity por
    HTTP en cada comando y que Rentals siguiera funcionando si Identity está caído; el precio es consistencia eventual.
  - **Por qué sobra.** Cuando alguien pide una visita se le crea su usuario, y ese usuario ya trae el teléfono
    verificado, así que la regla se puede validar contra la cuenta —o contra el propio flujo de creación del
    usuario— en vez de contra una réplica que hay que mantener sincronizada.
  - **Orden.** Primero cambiar la regla en visitas; solo después borrar la réplica.
  - **Qué se borra al final.** `IIdentityReplica`, `PersonVerification`, `MartenIdentityReplica`,
    `PersonVerificationDocument`, `PersonVerificationChangedHandler`, la cola `rentals-person-verification` en
    `RabbitMqMessagingExtensions`, el índice de `MartenStoreConfiguration`, el registro en `DependencyInjection`,
    y `DevIdentityEndpoints` con sus contratos `DevIdentityResponse` y `DevPersonVerificationRequest` (la página
    `/rentals/identity` de la Web ya no existe). `ListingAccess` se queda sin dependencias y vuelve a ser estático.
  - **De dónde viene el nombre.** `Person` e `IsDocumentVerified` —que ningún mensaje llena, solo el endpoint dev—
    venían del servicio de Personas que iba a reemplazar esto, también pendiente más abajo.
- [ ] Reiniciar la base de Rentals (esquema `rentals` de Marten): los flujos guardados tienen eventos que ya no existen (ofertas, reservas, reportes, asesor, consultas, favoritos, búsquedas) y `ListingDrafted` cambió de forma, así que no se pueden reconstruir.
- [ ] Rol `Asesor` en la base de Identity: el seeder crea `Moderador`, pero el rol viejo y sus asignaciones siguen ahí. Borrarlo o migrar a sus usuarios.
- [ ] Al vencer un `Listing` (`ListingExpired`), cancelar sus visitas futuras y avisar a los visitantes. Al cerrarlo o retirarlo ya se hace (`ListingLifecycleProcess.OnNoLongerAvailableAsync`).
- [ ] Reportes de publicaciones como agregado propio (antes vivían en `Listing`: un reporte por usuario y suspensión automática al tercero).
- [ ] Publicar un evento hacia facturación cuando un `Listing` se arrienda o se vende (`ListingClosed`).
- [ ] **Rediseñar la creación de `Owner` y su cuenta.** Hoy Rentals pide la cuenta por el bus (`AccountCreationRequested` → cola `identity-accounts`) y eso se salta la verificación de credenciales.
  - **Qué está mal hoy.** `CreateAccountHandler` (Identity) crea la cuenta con el canal ya confirmado, sin código; si ya existe un usuario con ese correo le sobrescribe perfil, contacto y rol; y cuando el propietario se registra a sí mismo pide una cuenta que ya existe.
  - **Principio.** Identity es el único que crea usuarios, siempre por su registro con verificación. Rentals solo crea `Owner`, y la prueba de identidad es el token.
  - **Propietario con cuenta.** Se registra en Identity desde el backoffice (rol Propietario por el cliente OIDC), entra, y si no tiene owner el backoffice lo lleva a "Completa tu perfil de propietario", que llama a `POST /api/owners/natural` o `/company` con su token (mismo patrón que `CompleteVisitorProfile`).
  - **Propietario sin cuenta.** Lo crea un administrador o moderador a nombre de un tercero, sin cuenta ni mensaje. Mientras no tenga cuenta no recibe las notificaciones de sus avisos ni puede atender visitas (el anfitrión es `OwnerId`).
  - **Vínculo.** `Owner.Id` siempre propio (nunca el id del usuario); `Owner` gana un `UserId` opcional y el evento `OwnerClaimed`. Reserva de unicidad por `UserId`, como `OwnerDocumentReservation`.
  - **Reconocer a un owner que ya existía.** Cuando alguien entra sin owner, buscar owners sin `UserId` cuyo correo o teléfono coincida con el contacto **verificado** del token. Una coincidencia: se le muestra y al confirmar se emite `OwnerClaimed`. Ninguna: formulario de perfil. Varias: lo resuelve un moderador. La reserva por documento queda como segunda red, pero una cédula repetida no enlaza sola: va a moderación.
  - **Identity.** Faltan los claims `email_verified` y `phone_number_verified` (`OidcPrincipalFactory` hoy pone correo y teléfono sin decir cuál está verificado) y comprobar que lleguen al access token de Rentals (dependen de los scopes `email` y `phone`).
  - **Rentals.** `OwnerIdResolver` siempre genera id nuevo; `DraftListingHandler` y `GetOwnerByIdHandler` comparan el `UserId` del owner en vez de `OwnerId` con `userId`; comando para reclamar y consulta del owner propio (`GET /api/owners/me`).
  - **Qué se borra.** `IAccountProvisioner`, `WolverineAccountProvisioner`, `AccountCreationRequestedHandler`, la cola `identity-accounts`, `account_creation_requested.proto` y `CreateAccountHandler` si nadie más lo usa.
  - **Por decidir.** Si una misma persona puede tener dos owners (ella como natural y su empresa); hoy el código asume uno.
  - **Descartado.** `Owner` como entidad dentro de `Listing` (y `Visitor` dentro de `Visit`): se pierde la unicidad por documento, un cambio de contacto son N escrituras, el owner no puede existir sin publicación y los datos personales quedan copiados en cada stream.
- [ ] **Consultas** (`Inquiry`, un flujo por par publicación–interesado, id UUID v5 de `ListingId + SeekerId`).
  - Una sola conversación por interesado y publicación; un mensaje nuevo se añade a la existente.
  - Mensajes de 1 a 1.000 caracteres, sin teléfonos, correos ni enlaces: el contacto pasa por la plataforma.
  - Máximo 10 consultas nuevas por interesado al día (regla blanda, contra una proyección).
  - Solo en publicaciones publicadas; el dueño no puede consultar la suya; el interesado necesita teléfono verificado.
  - Solo el propietario responde. Se cierran cuando la publicación se cierra, se retira o vence.
  - Eventos: `InquiryOpened(InquiryId, ListingId, SeekerId, Message)`, `InquiryMessagePosted(AuthorId, Text)`, `InquiryClosed(Reason)`.
  - Notificaciones `consulta.nueva` (al propietario) y `consulta.respuesta` (al interesado).
  - Mostrar públicamente el tiempo de respuesta del propietario (solo lado de lectura).
- [ ] **Favoritos** (`FavoriteList`, un flujo por usuario).
  - Máximo 100 por usuario; solo se agregan publicaciones publicadas; si cambian de estado siguen en la lista mostrando el estado actual.
  - Agregar o quitar dos veces lo mismo no genera evento. Eventos: `ListingFavorited(ListingId)`, `ListingUnfavorited(ListingId)`.
  - Avisar a quienes la tienen en favoritos cuando el precio baja 3% o más (`favorito.precio.baja`) y cuando deja de estar disponible (`favorito.no.disponible`).
  - Mostrar el número de favoritos en la ficha.
- [ ] **Búsquedas guardadas** (`SavedSearchList`, un flujo por usuario, con `SavedSearch` como entidad hija).
  - Máximo 10 por usuario. Cada una tiene nombre, criterios (operación, tipo, ciudad, barrio, rango de precio, habitaciones…) y frecuencia de alertas: inmediata, diaria (7:00 hora del usuario) o ninguna.
  - Eventos: `SavedSearchCreated(SearchId, Name, Criteria, Frequency)`, `SavedSearchUpdated(...)`, `SavedSearchDeleted(SearchId)`.
  - Alertas inmediatas al aprobar una publicación o bajar su precio (`busqueda.coincidencia`); resumen diario con las publicaciones nuevas que coinciden (`busqueda.resumen`), con una tarea recurrente.
  - Solo se envían alertas con el consentimiento `Alertas` vigente en Identity (se quitó la réplica `AlertsConsentChanged`; hay que volver a publicarla desde Identity).

## Configuración y limpieza

- [ ] Buggregator: quitar `localhost:1025` y `localhost:8000` fijos del appsettings de Notification y tomarlos de Aspire.
- [ ] Redirect URIs y audiences con puertos fijos en `AppHost/appsettings.json`: mantenerlos alineados con los `launchSettings`.
- [ ] Actualizar los nombres viejos (`apiservice`, `webfrontend`) en las páginas de tutoriales y en `CLAUDE.md`.
- [ ] Actualizar `Person.md`, que describe el diseño anterior.
- [ ] Tests para Identity, el login de la Web y el outbox.

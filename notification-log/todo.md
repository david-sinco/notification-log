# Pendientes

## Identity

- [ ] UI de administración de usuarios en la Web: listado, roles, bloquear y desbloquear.
- [ ] Agregar o cambiar el correo o el celular de una cuenta existente, con código y evento `UserContactChanged`.
- [ ] Recuperar y cambiar la contraseña.
- [ ] Páginas de error del protocolo, acceso denegado y confirmación de cierre de sesión.
- [ ] Apariencia por cliente en el login, según el `client_id` validado.

## Web

- [ ] **Compartir la hoja de estilos de "Llave".** Los mismos tokens de marca viven duplicados en `IdentityService.Api/wwwroot/css/llave.css` (login y registro), en `Web/wwwroot/app.css` y en `UI/portal/app/globals.css`; unificarlos para que un cambio de marca se haga en un solo sitio.
- [ ] **Título del inmueble dentro de `VisitView`.** `VisitViewProjection` copia tipo, barrio y ciudad de la publicación cuando se pide la visita; si después llega un `ListingDetailsUpdated`, las visitas ya existentes conservan el título anterior. Propagarlo a las visitas de esa publicación.
- [ ] **Nombre de quien creó la publicación.** `ListingView.CreatedBy` es un id de usuario de Identity y Rentals no conoce nombres de usuarios; la Web muestra «Tú», el propietario o el id corto.
- [ ] **Renovar el access token dentro del circuito.** `CookieOidcRefresher` solo renueva en una petición HTTP. Con `InteractiveServer` global, un circuito abierto más de 15 min (vida del access token) sin recargar la página sigue enviando el token vencido y las APIs responden 401.

## Portal (Next.js)

- [ ] Renovar el access token con refresh token; hoy, cuando vence (1 h), la sesión del portal termina y hay que volver a iniciar sesión (Identity la recuerda, así que es solo una redirección).
- [ ] El catálogo público devuelve `ListingDto` completo (incluye dirección y `OwnerId`); decidir qué datos se muestran sin iniciar sesión.

## Integración y negocio

- [ ] **Log de eventos reproducible** (estilo Kafka) para reconstruir estado o alimentar servicios nuevos; hoy RabbitMQ solo garantiza la entrega.
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
  - **Reglas que no se validan todavía** (todas consultan varias visitas a la vez, contra una proyección):
    - máximo de visitas activas por visitante;
    - una sola visita activa por visitante y publicación (hoy se puede pedir dos veces la misma);
    - bloqueo por cancelaciones tardías o inasistencias repetidas (`IsLateCancellation`, `NoShow`);
    - que el anfitrión no agende dos visitas a la misma hora.
- [ ] **Cancelar las visitas cuando la publicación deja de recibirlas.** Hoy ninguna visita se cancela sola: solo existe `CancelVisitHandler`, que es la acción del usuario.
  - **Estados sin visitas activas.** `Paused` (`ListingPaused`), `Closed` (`ListingClosed`), `Withdrawn` (`ListingWithdrawn`), `Suspended` (`ListingSuspended`) y `Expired` (`ListingExpired`). `Draft` e `InReview` nunca las tienen, porque solo se piden en `Published`.
  - **Cómo.** Un proceso que reaccione a esos eventos, busque las visitas activas de la publicación (proyección por `ListingId`) y llame a `visit.Cancel(VisitParty.System, ...)` en cada una, avisando a las dos partes.
  - **Reanudar o rehabilitar** (`ListingResumed`, `ListingReinstated`) no revive las visitas canceladas; el visitante vuelve a pedirlas.
  - **Programación.** Solo el caso de `Expired` depende de *Schedules*, porque `ListingExpired` debe emitirlo una tarea programada. Pausar, cerrar, retirar y suspender los dispara una persona, así que esos se pueden hacer ya.
- [ ] **Datos de la publicación y filtros del catálogo.** El análisis completo está en `propertyDetail.md`.
  - **Fase 1.** Normalizar ciudad y barrio, agregar `PublishedAt` a `ListingView` con ordenamiento en el catálogo, y separar `CatalogCriteria` de `ListingFilter` con los filtros de prioridad alta.
  - **Fase 2.** Título, área privada y construida, antigüedad y `RentalTerms` (disponible desde, mascotas, administración incluida).
  - **Fase 3.** Características del inmueble y amenidades del conjunto.
  - **Fase 4.** Coordenadas, búsqueda por mapa y ubicación aproximada en el catálogo público (detallada en el pendiente del mapa, abajo).
  - **Decidir.** Eventos por sección o ampliar `ListingDetailsUpdated`, qué cambios vuelven a revisión, y qué pasa con las publicaciones ya publicadas a las que les faltan datos obligatorios nuevos.
- [ ] **Mapa de publicaciones con Leaflet.** El análisis completo está en `listingMap.md`.
  - **Dominio y lectura.** VO `GeoPoint`, evento `ListingLocated` y comando `LocateListing`. `ListingView` guarda la coordenada exacta y una aproximada (desplazamiento determinista de unos 200–300 m); el catálogo público solo devuelve la aproximada.
  - **Web.** Componente `LocationPicker` con un módulo JS propio de Leaflet, para que el propietario o el moderador arrastre el pin. El mapa se centra geocodificando la ciudad y el barrio, no la dirección.
  - **Portal.** `react-leaflet` sin SSR: círculo aproximado en la ficha y vista de mapa en el catálogo, con filtro por zona visible, endpoint `GET /api/catalog/map` y agrupación de marcadores.
  - **Teselas.** OSM solo en desarrollo; la URL y la atribución van en configuración para cambiar de proveedor sin tocar código.
  - **Decidir.** Si el pin es obligatorio para enviar a revisión, si moverlo devuelve la publicación a revisión, y si mostrar la ubicación exacta en la visita ya agendada.
- [ ] Reportes de publicaciones como agregado propio (antes vivían en `Listing`: un reporte por usuario y suspensión automática al tercero).
- [ ] Publicar un evento hacia facturación cuando un `Listing` se arrienda o se vende (`ListingClosed`).
- [ ] **Owner reclamable: lo que quedó pendiente.** Rentals ya no crea cuentas: `Owner.Id` siempre es nuevo, el que registra un moderador queda con `RelatedUserId` vacío y lo reclama (`POST /api/owners/{id}/claim`, evento `OwnerClaimed`) quien confirmó su correo o su teléfono.
  - **Anfitrión sin usuario.** Mientras nadie reclama al owner, `Owner.HostUserId` cae en `CreatedBy`: quien lo registró es la parte anfitriona de las visitas y recibe sus notificaciones. Las visitas ya creadas no cambian de anfitrión al reclamarse. Confirmar que es lo esperado.
  - **Un owner por usuario.** La reserva `OwnerUserReservation` impide registrarse o reclamar si el usuario ya tiene owner. Decidir si una persona puede tener dos (ella como natural y su empresa).
  - **Documentación desactualizada.** `UI/docs` importa con `?raw` `CreateAccountHandler.cs` y `AccountCreationRequestedHandler.cs`, que se borraron (`arquitectura/recorrido.mdx`, `oauth/cuentas.mdx`, `patrones/adaptador.mdx`, `patrones/outbox.mdx`), y describe la cola `identity-accounts` en `arquitectura/{index,servicios,mensajeria,entorno}.mdx` y `oauth/{index,decisiones}.mdx`. Reescribir esas páginas con el flujo de reclamo.
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

- [ ] Redirect URIs y audiences con puertos fijos en `AppHost/appsettings.json`: mantenerlos alineados con los `launchSettings`.
- [ ] Tests para Identity, el login de la Web y el outbox.

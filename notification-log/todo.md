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
- [ ] **Pruebas de carga del Web en Blazor.** Los circuitos de SignalR no se prueban con HTTP simple; usar el módulo de navegador de k6 con 20 a 50 usuarios como un perfil más en `LoadTests/`.
- [ ] **Renovar el access token dentro del circuito.** `CookieOidcRefresher` solo renueva en una petición HTTP. Con `InteractiveServer` global, un circuito abierto más de 15 min (vida del access token) sin recargar la página sigue enviando el token vencido y las APIs responden 401.

## Portal (Next.js)

- [ ] Renovar el access token con refresh token; hoy, cuando vence (1 h), la sesión del portal termina y hay que volver a iniciar sesión (Identity la recuerda, así que es solo una redirección).
- [ ] El catálogo público devuelve `ListingDto` completo (incluye dirección y `OwnerId`); decidir qué datos se muestran sin iniciar sesión.

## Integración y negocio

- [ ] **Log de eventos reproducible** (estilo Kafka) para reconstruir estado o alimentar servicios nuevos; hoy RabbitMQ solo garantiza la entrega.

## Rentals

- [ ] **Jobs: trabajos asíncronos programados** (vencer publicaciones, avisar las que están por vencer; después visitas y resúmenes).
  - **Estado actual.** `ApplyExpiry`, `ICommandScheduler`, `WolverineCommandScheduler` y `ScheduledCommandsHandler` ya se quitaron. `ExpireListingCommand` y `ExpireListingHandler` siguen registrados, pero nada los dispara: hoy ninguna publicación vence sola, salvo al rehabilitarla (`Reinstate` llama a `Expire`).
  - **Enfoque: barrido, no programar por adelantado.** El estado ya dice qué toca (`ExpiresAt` en el agregado y en `ListingView`), así que no hay schedules que guardar, cancelar ni deduplicar. Se descartó la tabla compartida de schedules con locks y los mensajes programados de Wolverine.
  - **Proyecto `Jobs/NotificationLog.Jobs`.** App de consola que ejecuta un job (`--job <nombre>`) y termina, con código de salida 0 o 1. Pensada para Cloud Run Jobs o Container Apps Jobs: misma imagen, un job por argumento y cron. No referencia proyectos de los servicios (solo `Contracts` y `ServiceDefaults`) y no tiene base de datos ni outbox.
  - **Cómo trabaja un job.** Consulta un endpoint interno del servicio dueño (client credentials, scope propio como `rentals.jobs`) que devuelve solo ids, y publica un comando por id a RabbitMQ. El servicio ejecuta el comando recargando el agregado.
  - **Por qué no hay condición de carrera.** La vista solo sugiere candidatos; el agregado decide (`Expire` no hace nada si ya se renovó o no está visible). Las escrituras simultáneas (otra instancia, el dueño renovando) las rechaza la concurrencia optimista de `AggregateStreams`; el consumer reintenta y el agregado vuelve a decidir. Si un mensaje se pierde, el próximo barrido lo vuelve a encontrar: el barrido es el reintento.
  - **Jobs iniciales.** `rentals.listings.expire` (cada ~10 min) y `rentals.listings.warn-expiry` (una vez al día, por ejemplo 8:00).
  - **Aviso de vencimiento en el dominio.** Como el job no tiene estado, el agregado recuerda que ya avisó: `Listing.WarnExpiry(now)` emite `ListingExpiryWarned(ExpiresAt)` solo si está visible, dentro de `RenewalWindow` y sin aviso para ese `ExpiresAt`. `ListingView.ExpiryWarnedFor` lo excluye del barrido; al renovar cambia `ExpiresAt` y vuelve a avisar. La notificación sale del handler en Rentals, no del job.
  - **Piezas.** Contracts: `ExpireListing` y `WarnListingExpiry`. Rentals: `WarnExpiry` + evento + prueba, `ExpiryWarnedFor` en la vista, comando `WarnListingExpiry`, dos consumers con sus colas y los endpoints internos `expiry-due` y `expiry-warning-due`. Identity: cliente `jobs` con el scope `rentals.jobs`. Jobs: proyecto, `IJob`, cliente de Rentals y los dos jobs. AppHost: un recurso por job con `WithArgs("--job", ...)` y `WithExplicitStart()`.
  - **Arranque.** Marten y Wolverine no van en el job, así que el arranque en frío debería ser corto; revisar si Wolverine solo con RabbitMQ o `RabbitMQ.Client` directo.
- [ ] **Outbox para las notificaciones de Rentals.** Todos los handlers publican con `IMessageBus` después de `SaveChangesAsync`, fuera de la transacción: si el proceso se cae entre las dos líneas, el evento queda guardado y el aviso se pierde (el reintento no lo repite porque el agregado ya cambió). Usar `IMartenOutbox` enrolado en la sesión en `WolverineNotificationDispatcher` y publicar antes del commit.
- [ ] **Visitas: tareas programadas, cancelación automática y reglas sin validar.** El dominio (`Visit`) y los comandos de usuario ya existen; falta lo que no dispara una persona.
  - **Vencer la visita sin respuesta.** Al llegar el `RespondBy` de cada propuesta, llamar a `visit.Expire(now)` (emite `VisitExpired`). El método ya existe; faltan el comando `ExpireVisit` y programarlo tras `VisitRequested` y `VisitCounterProposed`. Avisar a las dos partes (`visita.vencida`).
  - **Cierre automático.** 72 h después de que termine la franja agendada, llamar a `visit.AutoComplete(now)` (emite `VisitCompleted` con `System`). El método ya existe; faltan el comando y programarlo tras `VisitScheduled`.
  - **Recordatorios de la visita agendada** a las dos partes (por ejemplo, 24 h antes). Solo notifican, no cambian el estado (`visita.recordatorio`).
  - Los dos métodos del dominio no hacen nada si todavía no toca o la visita ya cambió de estado, así que un mensaje viejo o repetido es inofensivo. Encaja con el pendiente de *Jobs* de arriba (barrido por `RespondBy` y por fin de la franja).
  - **Reglas que no se validan todavía** (todas consultan varias visitas a la vez, contra una proyección):
    - máximo de visitas activas por visitante;
    - una sola visita activa por visitante y publicación (hoy se puede pedir dos veces la misma);
    - bloqueo por cancelaciones tardías o inasistencias repetidas (`IsLateCancellation`, `NoShow`);
    - que el anfitrión no agende dos visitas a la misma hora.
- [ ] **Cancelar las visitas cuando la publicación deja de recibirlas.** Hoy ninguna visita se cancela sola: solo existe `CancelVisitHandler`, que es la acción del usuario.
  - **Estados sin visitas activas.** `Paused` (`ListingPaused`), `Closed` (`ListingClosed`), `Withdrawn` (`ListingWithdrawn`), `Suspended` (`ListingSuspended`) y `Expired` (`ListingExpired`). `Draft` e `InReview` nunca las tienen, porque solo se piden en `Published`.
  - **Cómo.** Un proceso que reaccione a esos eventos, busque las visitas activas de la publicación (proyección por `ListingId`) y llame a `visit.Cancel(VisitParty.System, ...)` en cada una, avisando a las dos partes.
  - **Reanudar o rehabilitar** (`ListingResumed`, `ListingReinstated`) no revive las visitas canceladas; el visitante vuelve a pedirlas.
  - **Programación.** Solo el caso de `Expired` depende de *Jobs*, porque `ListingExpired` lo emite el job de vencimiento. Pausar, cerrar, retirar y suspender los dispara una persona, así que esos se pueden hacer ya.
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
  - **Lo difícil es la búsqueda inversa.** Dada una publicación aprobada o con precio rebajado, encontrar todas las búsquedas guardadas que coinciden y avisar a cada dueño; necesita una proyección de búsquedas indexada por criterios. Aplica a la alerta inmediata y al resumen. El resumen diario además es una tarea recurrente para todos los usuarios, distinta de los schedules puntuales de un agregado.

## Configuración y limpieza

- [ ] Redirect URIs y audiences con puertos fijos en `AppHost/appsettings.json`: mantenerlos alineados con los `launchSettings`.
- [ ] Tests para Identity, el login de la Web y el outbox.
- [ ] **Documentación del scheduler borrado.** `UI/docs/.../event-sourcing/procesos.mdx` importa con `?raw` `ICommandScheduler.cs`, `WolverineCommandScheduler.cs` y `ScheduledCommandsHandler.cs`, que ya no existen, y `tareas.mdx` todavía menciona `ICommandScheduler`. Reescribirlas con el enfoque de *Jobs*.

# Pendientes

## Prioridad alta

- [ ] **Token hacia las APIs.** Enviar el access token desde la Web con un `DelegatingHandler` que agregue `Authorization: Bearer` y lo renueve con el refresh token (patrón `BlazorWebAppOidcServer`).
- [ ] **Caída de Notification y Rentals con el debugger** (`ExecutionEngineException`). Confirmar arrancando sin depurar; salidas: desactivar Hot Reload o pregenerar el código de Wolverine (`TypeLoadMode.Static`).
- [ ] **Validar tokens en Rentals y Notification.**
  - OpenIddict Validation con su audience, más `LocalhostSubdomainHandler` para llegar a Identity.
  - Policies por rol.
  - Actor desde `sub` en lugar de `ActorId` en el body.
  - Rutas `/api/me/...` en lugar de `/api/users/{userId}/...`.

## Identity

- [ ] UI de administración de usuarios en la Web: listado, roles, bloquear y desbloquear.
- [ ] Agregar o cambiar el correo o el celular de una cuenta existente, con código y evento `UserContactChanged`.
- [ ] Recuperar y cambiar la contraseña.
- [ ] Páginas de error del protocolo, acceso denegado y confirmación de cierre de sesión.
- [ ] Apariencia por cliente en el login, según el `client_id` validado.
- [ ] Scalar con OAuth para probar `/api/users` sin la Web.

## Integración y negocio

- [ ] **Log de eventos reproducible** (estilo Kafka) para reconstruir estado o alimentar servicios nuevos; hoy RabbitMQ solo garantiza la entrega.
- [ ] **Servicio de Personas** (con capas): `Person` con o sin cuenta, perfil de asesor, consentimientos y verificación de documento. Reemplaza la réplica dev de `/rentals/identity`.
- [ ] Responder 403 en lugar de 400 para "no es tuyo" en Rentals (`ForbiddenException`).

## Rentals

- [ ] Ajustar Infrastructure, Api y Web al nuevo dominio y a la Application de `Listing` (sin ofertas, reservas, reportes ni asesor; `OwnerId` + `CreatedBy`; los comandos de publicaciones reciben el `ClaimsPrincipal` en lugar de `ActorId`/`ModeratorId`).
- [ ] Quitar lo que queda del asesor: asignación en `ListingDetail.razor`, réplica `AdvisorDocument` / `AdvisorChanged` (Infrastructure, Contracts, endpoints dev e `IdentityReplica.razor`) y el `AdvisorId` de `DraftListingRequest`.
- [ ] Rol `Asesor` en la base de Identity: el seeder crea `Moderador`, pero el rol viejo y sus asignaciones siguen ahí. Borrarlo o migrar a sus usuarios.
- [ ] Al vencer un `Listing` (`ListingExpired`), cancelar sus visitas futuras y avisar a los visitantes. Al cerrarlo o retirarlo ya se hace (`ListingLifecycleProcess.OnNoLongerAvailableAsync`).
- [ ] Reportes de publicaciones como agregado propio (antes vivían en `Listing`: un reporte por usuario y suspensión automática al tercero).
- [ ] Publicar un evento hacia facturación cuando un `Listing` se arrienda o se vende (`ListingClosed`).
- [ ] **Usuario por cada `Owner`.** Al registrar un propietario (también si lo registra un moderador), crear su usuario en Identity para confirmar correo o teléfono y poder enviarle notificaciones. Mientras tanto, un `Owner` sin cuenta no recibe las notificaciones de sus avisos ni puede atender visitas (el anfitrión es `OwnerId`).
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

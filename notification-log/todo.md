# Pendientes

## Prioridad alta

- [ ] **Token hacia las APIs.** Enviar el access token desde la Web con un `DelegatingHandler` que agregue `Authorization: Bearer` y lo renueve con el refresh token (patrón `BlazorWebAppOidcServer`).
- [ ] **Validar tokens en Rentals y Notification.**
  - OpenIddict Validation con su audience, más `LocalhostSubdomainHandler` para llegar a Identity.
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

- [ ] **Compartir la hoja de estilos de "Llave".** Los mismos tokens de marca viven duplicados en `IdentityService.Api/wwwroot/css/llave.css` (login y registro) y en `Web/wwwroot/app.css`; unificarlos para que un cambio de marca se haga en un solo sitio.
- [ ] Página propia de acceso denegado: `/authentication/access-denied` todavía responde texto plano.

## Integración y negocio

- [ ] **Log de eventos reproducible** (estilo Kafka) para reconstruir estado o alimentar servicios nuevos; hoy RabbitMQ solo garantiza la entrega.
- [ ] **Servicio de Personas** (con capas): `Person` con o sin cuenta, consentimientos y verificación de documento. Reemplaza la réplica dev de `/rentals/identity`.
- [ ] Responder 403 en lugar de 400 para "no es tuyo" en las visitas de Rentals (`ForbiddenException`); publicaciones y propietarios ya lo hacen.

## Rentals

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
- [ ] **Usuario por cada `Owner`.** Al registrar un propietario (también si lo registra un moderador), crear su usuario en Identity para confirmar correo o teléfono y poder enviarle notificaciones. Mientras tanto, un `Owner` sin cuenta no recibe las notificaciones de sus avisos ni puede atender visitas (el anfitrión es `OwnerId`).
- [ ] **Mismo id para el usuario y su `Owner`.** Al crear un usuario con rol de propietario hay que registrarlo en Owners con ese mismo id. Si el `Owner` ya existía, actualizar su id para que coincida con el del usuario, o crear el usuario con el id que ya tiene el `Owner`.
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

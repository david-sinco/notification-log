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

## Configuración y limpieza

- [ ] Buggregator: quitar `localhost:1025` y `localhost:8000` fijos del appsettings de Notification y tomarlos de Aspire.
- [ ] Redirect URIs y audiences con puertos fijos en `AppHost/appsettings.json`: mantenerlos alineados con los `launchSettings`.
- [ ] Actualizar los nombres viejos (`apiservice`, `webfrontend`) en las páginas de tutoriales y en `CLAUDE.md`.
- [ ] Actualizar `Person.md`, que describe el diseño anterior.
- [ ] Tests para Identity, el login de la Web y el outbox.

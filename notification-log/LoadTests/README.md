# Pruebas de carga

Scripts de [k6](https://grafana.com/docs/k6/) que se ejecutan como contenedores del AppHost de Aspire.

## Cómo se corren

1. Levanta el AppHost como siempre (`dotnet run --project NotificationLog.AppHost`).
2. En el dashboard de Aspire, grupo `loadtests`, dale play al perfil:

| Recurso | Usuarios virtuales | Duración aproximada |
|---|---|---|
| `loadtest-smoke` | 1 por escenario | 1.5 min |
| `loadtest-100` | 100 | 13 min |
| `loadtest-1000` | 1000 | 22 min |

Mientras corre, el endpoint `dashboard` del recurso abre el web dashboard de k6 con las gráficas en vivo. Al terminar quedan en `LoadTests/results/`:

- `<perfil>-report.html`: reporte HTML.
- `<perfil>-summary.json`: resumen completo con todas las métricas.

Para ver **dónde** se va el tiempo de una petición lenta, usa las trazas del dashboard de Aspire (OpenTelemetry de cada servicio).

## Qué se mide

| Escenario | Mezcla | Qué hace |
|---|---|---|
| `catalog` | 60 % | Visitante anónimo: busca en el catálogo y abre publicaciones |
| `visitor` | 20 % | Visitante autenticado: mira el catálogo, pide una visita y lista sus visitas |
| `owner` | 12 % | Propietario: dashboard y listados; crea una publicación con detalles, precio y 5 fotos, y la envía a revisión |
| `negotiation` | 5 % | Visitante y anfitrión negocian una visita (solicitud, contrapropuesta, agenda) y la cancelan **los dos a la vez** |
| `moderator` | 3 % | Aprueba publicaciones en revisión y consulta propietarios y visitantes |
| `token` | tasa fija | Solo pide tokens a Identity (hash de contraseña) |
| `notifications` | 1 cada 10 s | Sonda: cambia el precio de un borrador y mide cuánto tarda la notificación en registrarse en Notification |

Límites de aceptación (si alguno se supera, k6 termina con código 99):

- p95 por debajo de 500 ms en lecturas y por debajo de 1 s en escrituras y tokens.
- Menos del 1 % de errores.
- p95 de `notification_lag` por debajo de 5 s, sin notificaciones perdidas en 60 s (`notification_timeouts`).

`concurrency_conflicts` cuenta los 500 de la cancelación simultánea: Rentals responde 500 cuando Marten detecta un conflicto de concurrencia optimista.

## Datos de prueba

Los servicios no saben que existen estas pruebas: el `setup()` de k6 crea sus datos por los flujos reales.

- **Usuarios.** Se registran por las páginas de Identity (`/Account/Register` y `/Account/Verify`), igual que una persona. El código de verificación se lee del correo que llega a Buggregator. Solo se registran los que el perfil necesita y que todavía no existen:
  - visitantes `visitor-0001@loadtest.local` en adelante (los de `visitor` y luego los de `negotiation`);
  - propietarios `owner-0001@loadtest.local` en adelante, registrados desde el cliente `web` para que reciban el rol de propietario;
  - hasta 5 moderadores `moderator-0001@loadtest.local`, registrados como visitantes y promovidos con `PUT /api/users/{id}/role`.

  Todos usan la contraseña `LoadTest12345`.
- **Administrador.** Se usa el que siembra Identity en desarrollo (`admin@notificationlog.local`; se cambia con `ADMIN_EMAIL` y `ADMIN_PASSWORD`).
- **Tokens.** k6 pide los tokens con el password grant del cliente `loadtest`. Identity solo habilita ese grant y crea ese cliente cuando corre en `Development`.
- **Réplicas.** Antes de empezar, el setup espera a que Rentals conozca al último visitante y Notification al propietario de la sonda; si no llegan en 2 minutos, la prueba termina con un error.
- **Publicaciones.** El setup registra los propietarios en Rentals y crea las publicaciones aprobadas que falten, a través de la API. Todas llevan `LoadTest` en el barrio y la descripción.
- **Sonda de notificaciones.** Si no existe el trigger `publicacion.precio.cambiado`, el setup lo crea con la plantilla `pruebas_carga_precio_cambiado`.

Los datos quedan en las bases de desarrollo y se reutilizan en la siguiente corrida.

## Estructura

```
k6/
├── main.js          opciones, escenarios, umbrales y resumen
├── profiles/        smoke, carga-100 y carga-1000
├── scenarios/       un archivo por escenario
└── lib/             configuración, tokens, peticiones, datos y setup
results/             reportes (ignorados por git)
```

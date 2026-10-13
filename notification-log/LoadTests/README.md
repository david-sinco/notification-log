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

- `<destino>-<perfil>-report.html`: reporte HTML.
- `<destino>-<perfil>-summary.json`: resumen completo con todas las métricas.

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

- **Usuarios.** Identity los siembra al arrancar si `LoadTests:Enabled` es `true` en el AppHost (lo es por defecto). Son visitantes, propietarios, moderadores y un administrador, ya verificados (`visitor-0001@loadtest.local` … `visitor-1000@loadtest.local`, `owner-0001@loadtest.local` … `owner-0100@loadtest.local`, `moderator-0001@loadtest.local` … `moderator-0005@loadtest.local`, `admin-0001@loadtest.local`), con contraseña `LoadTest12345`.
- **Password grant.** El mismo flag habilita el password grant para el cliente `loadtest`.
- **Publicaciones.** El `setup()` de k6 registra los propietarios en Rentals y crea las publicaciones aprobadas que falten, a través de la API. Todas llevan `LoadTest` en el barrio y la descripción.
- **Sonda de notificaciones.** Si no existe el trigger `publicacion.precio.cambiado`, el setup lo crea con la plantilla `pruebas_carga_precio_cambiado`.

Los datos quedan en las bases de desarrollo y se reutilizan en la siguiente corrida. Para no sembrar nada, pon `LoadTests:Enabled` en `false`.

## Contra un ambiente publicado

Agrega el ambiente en la configuración del AppHost (por ejemplo, en user secrets):

```json
"LoadTests": {
  "Environments": {
    "staging": {
      "IdentityUrl": "https://identity.staging.example",
      "RentalUrl": "https://rentals.staging.example",
      "NotificationUrl": "https://notifications.staging.example"
    }
  }
}
```

Aparecerán `loadtest-staging-100` y `loadtest-staging-1000`. Ese ambiente debe tener `Seed__LoadTest__Enabled=true` en Identity. Nunca lo actives en producción: habilita el password grant y crea usuarios con una contraseña conocida.

## Estructura

```
k6/
├── main.js          opciones, escenarios, umbrales y resumen
├── profiles/        smoke, carga-100 y carga-1000
├── scenarios/       un archivo por escenario
└── lib/             configuración, tokens, peticiones, datos y setup
results/             reportes (ignorados por git)
```

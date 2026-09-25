# Problemas de event sourcing

Casos reales de este repositorio para tener presentes al explicar event sourcing. Todos son de la misma
familia: **reglas o procesos que dependen de muchos agregados a la vez**.

## La raíz del problema

En event sourcing el estado de un agregado no está en una fila, sino en su flujo de eventos. La única
lectura que el modelo da gratis es **"dame el flujo con este id"**. Cualquier pregunta del tipo "¿cuáles
agregados cumplen X?" no tiene respuesta directa:

| Pregunta                                                  | Base relacional                        | Event sourcing                             |
|---                                                        |---                                     |---                                         |
| ¿Cómo está la publicación 42?                             | `SELECT ... WHERE Id = 42`             | cargar el flujo 42 y aplicar sus eventos   |
| ¿Qué publicaciones vencen entre el día X y el día Y?      | `WHERE ExpiresAt BETWEEN @x AND @y`    | no se puede sin una proyección             |
| ¿Qué visitas futuras tiene la publicación 42?             | `WHERE ListingId = 42 AND ...`         | no se puede sin una proyección             |
| Cancelar todas esas visitas en una sola operación         | un `UPDATE` en una transacción         | cargar y escribir N flujos, uno por uno    |

Para contestar la pregunta global hay que construir y mantener una **proyección**, que es otro modelo
derivado de los eventos. Cada proyección trae sus propias preguntas:

- **¿Inline o asíncrona?** Si es inline, está al día pero hace más lenta cada escritura. Si es asíncrona,
  hay una ventana en la que la proyección todavía no refleja lo último que pasó.
- **¿De lectura o de escritura?** Si una decisión de escritura se apoya en una vista pensada para la UI,
  esa vista queda atada a la escritura (ver `Rentals.md` §5: "las reglas blandas usan proyecciones de apoyo
  del lado de escritura").
- **¿Quién la mantiene?** Cada evento nuevo que afecte la pregunta tiene que llegar a la proyección. Si
  alguien lo olvida, la proyección miente en silencio.
- **Carreras.** La proyección se lee y, *después*, se escribe en otro flujo. Entre las dos cosas el mundo
  puede cambiar, y ninguna transacción cubre ambas.

## Caso 1: avisar que una publicación está por vencer

**La regla (RN-08).** Una publicación vence 60 días después de aprobarse o renovarse. Se avisa al
publicador 7 días antes.

**La pregunta natural** es global: "¿qué publicaciones vigentes tienen `ExpiresAt` entre ahora + 7 días y
ahora + 7 días + 1 hora?". En una base relacional es una consulta con un índice. En event sourcing, los
flujos de `Listing` no se pueden filtrar por fecha.

### Lo que se hizo primero: un mensaje programado por publicación

Al ver `ListingApproved` o `ListingRenewed`, `ListingLifecycleProcess` programaba con Wolverine un
`WarnListingExpiryCommand(ListingId, ExpiresAt)` para `ExpiresAt - 7 días`. Así la pregunta global se
evita: cada publicación se "acuerda" de sí misma.

Tiene sus propios costos:

- **Los mensajes programados no se cancelan.** Si la publicación se renueva, el mensaje viejo sigue en la
  cola. Por eso lleva el `ExpiresAt` y, al ejecutarse, recarga el flujo y se descarta si ya no coincide
  (`Rentals.md` §5, "los mensajes programados son idempotentes"). Lo mismo pasa si se cierra, se retira o
  se suspende: el handler tiene que repetir las reglas de "¿sigue vigente?".
- **Era un comando que no mandaba nada.** `WarnListingExpiryHandler` no cambiaba el estado del `Listing`,
  solo notificaba. Vivía en `Listings/Commands` por necesidad técnica, no porque fuera un comando.
- **La regla quedaba repartida.** Los 7 días estaban en `ListingPolicy.ExpiryNotice`, el cálculo en
  `ListingLifecycleProcess` y el chequeo de vigencia en el handler.

### Alternativas que se descartaron

- **Barrer el read model** (`ListingView`) cada cierto tiempo. Funciona, pero una vista de lectura
  eventualmente consistente termina decidiendo efectos, y hay que marcar a quién ya se avisó.
- **Una "reserva" o calendario de vencimientos** como proyección del lado de escritura. Duplica un dato que
  ya está en el flujo y hay que mantenerlo sincronizado con cada transición (pausar, suspender, rehabilitar,
  cerrar, retirar, renovar). Es repetir las reglas del agregado fuera de él.
- **Un saga con timeouts.** Resuelve el estado de los timers, pero es mucha infraestructura para un aviso.
- **Que el aviso sea un evento de dominio** (`ListingExpiryNoticed`). Mete en el flujo un evento que solo
  existe para notificar, y sigue necesitando un disparador programado.

### La solución: llevar la pregunta a quien la puede contestar

Rentals deja de programar el aviso. Notification tiene un agregado de **recordatorios**. Rentals le manda
la clave de la configuración, el id del flujo y el payload. Notification ya sabe qué enviar y cuánto antes
enviarlo. Con `ListingRenewed`, Rentals reemplaza el recordatorio (misma clave, mismo flujo), y al cerrar,
retirar o suspender la publicación lo cancela. El detalle está en `todo.md`.

La idea que hay que llevarse: **Notification es relacional**, así que "los recordatorios pendientes con
`DueAt <= ahora`" es una consulta indexada de una línea. La pregunta global no desaparece. Se mueve al
servicio cuyo almacenamiento la contesta bien.

## Caso 2: cancelar las visitas cuando la publicación deja de estar disponible

**La regla.** Si una publicación se cierra, se retira o vence, sus visitas pendientes o confirmadas que
todavía no ocurrieron se cancelan, y se avisa a los visitantes.

**Por qué las visitas no viven dentro de `Listing`.** Cada visita tiene su propio ciclo de vida (solicitud,
confirmación, cancelación, inasistencia, recordatorios) y la modifican el visitante y el anfitrión por
separado. Si fueran parte del flujo de `Listing`, cada acción sobre una visita competiría por la versión
del flujo de la publicación. La publicación sería un cuello de botella, y su flujo crecería con cada
visita. Por eso `Visit` era un agregado propio, con un flujo por visita.

**La consecuencia.** `Listing` no sabe qué visitas tiene. "Cancela las visitas futuras de la publicación
42" es otra vez una pregunta global.

### Cómo se resolvió (antes de quitar las visitas)

1. **Una proyección de decisión del lado de escritura.** `VisitDecisionProjection` mantenía un documento
   `VisitDecision` por visita con `ListingId`, `VisitorId`, `HostId`, estado y franja. No era una vista de
   la UI: existía solo para contestar preguntas de escritura.
2. **Un proceso reaccionaba al evento.** `DomainEventsSubscription` (una suscripción asíncrona de Marten)
   recibía `ListingClosed` o `ListingWithdrawn` y llamaba a `ListingLifecycleProcess.OnNoLongerAvailableAsync`.
3. **Buscaba los ids** con `IProcessLookups.FindUpcomingVisitIdsAsync(listingId)` contra `VisitDecision`.
4. **Cargaba y escribía cada visita**: por cada id, cargar el flujo, comprobar que siguiera `Requested` o
   `Confirmed` y que la franja no hubiera empezado, `visit.Cancel(...)` y agregar el evento. Todo en un
   `SaveChangesAsync` al final.

### Lo que lo hace difícil

- **No es rápido.** Son N lecturas de flujo y N escrituras para lo que en SQL es un `UPDATE`. Una
  publicación con muchas visitas hace una transacción grande.
- **No es inmediato.** La suscripción es asíncrona. Entre `ListingClosed` y las cancelaciones hay una
  ventana en la que las visitas siguen "confirmadas": el visitante las ve y el anfitrión recibe
  recordatorios de una visita que ya no va a ocurrir.
- **No es atómico con el cierre.** El cierre de la publicación ya se guardó. Si el proceso falla a mitad de
  camino, el evento termina en dead letter y algunas visitas quedan canceladas y otras no. Reintentarlo
  tiene que ser idempotente, y lo es solo porque cada visita vuelve a comprobar su estado.
- **Carrera con visitas nuevas.** `RequestVisitHandler` verifica `listing.Status == Published` y crea la
  visita en otro flujo. Si alguien pide una visita justo mientras se cierra la publicación, la visita se
  crea después de que el proceso ya buscó los ids, y queda viva. Ningún bloqueo cubre los dos flujos.
- **Concurrencia con cada visita.** Si el visitante cancela su visita al mismo tiempo, una de las dos
  escrituras falla por versión, y se cae todo el lote.
- **Cada nuevo motivo es otro caso que no hay que olvidar.** El cierre y el retiro estaban cubiertos, el
  vencimiento no (quedó como pendiente en `todo.md`). Si la suspensión también debe cancelar visitas, es
  otro caso más en el enrutador.
- **La proyección hay que mantenerla.** Cualquier evento nuevo de `Visit` que cambie el estado tiene que
  reflejarse en `VisitDecision`, o la búsqueda devolverá visitas que ya no aplican (o no devolverá las que
  sí).

### Alternativas y lo que cuestan

- **Guardar los ids de visita en `Listing`.** Se evita la proyección, pero cada solicitud de visita escribe
  en dos flujos y vuelve la contención sobre la publicación.
- **No cancelar: que la visita consulte la publicación.** Al confirmar, recordar o mostrar una visita, se
  mira el estado de la publicación y se trata como cancelada si ya no está disponible. No hay lote ni
  ventana, pero el estado "cancelada" no queda registrado como evento, y cada lectura y cada proceso tienen
  que acordarse de hacer la comprobación.
- **Procesar las visitas una por una** (un mensaje por visita en lugar de un lote). Un fallo no arrastra a
  las demás y se reintenta solo esa, pero sigue siendo eventual y sigue necesitando la proyección para
  saber a quién mandar los mensajes.

## Otros casos de la misma familia en el repositorio

Las reglas blandas de `Rentals.md` §3 son el mismo problema en versión validación:

- **Máximo de visitas pendientes por visitante** (`ISoftRuleChecks.CountPendingVisitsAsync`): contar
  flujos de `Visit` por `VisitorId`.
- **Que el anfitrión no tenga dos visitas confirmadas a la misma hora** (`HostHasOverlapAsync`): buscar por
  rango de fechas entre flujos.
- **Suspensión por cancelaciones tardías o inasistencias** (`GetVisitStrikesAsync`): reunir hechos de
  muchas visitas de un mismo visitante.
- **Máximo 10 consultas nuevas por interesado al día** (pendiente en `todo.md`).

Todas se validan contra una proyección de escritura y **pueden fallar en una carrera** entre dos
peticiones simultáneas. Es un riesgo aceptado a propósito, y es parte de lo que hay que contar: en event
sourcing, las invariantes que cruzan agregados pasan a ser **eventualmente consistentes**, salvo que se
rediseñen los límites del agregado.

## Qué contar al presentarlo

- Event sourcing es excelente para responder "¿qué le pasó a este agregado y por qué?", y malo para
  "¿cuáles cumplen X?".
- Toda pregunta global necesita una proyección, y cada proyección es código que hay que mantener, con una
  decisión de consistencia (inline o asíncrona) y un dueño.
- Las acciones sobre muchos agregados (cancelar N visitas) son N transacciones pequeñas y eventuales, no
  una grande y atómica. Hay que diseñar para reintentos, idempotencia y ventanas de inconsistencia.
- A veces la mejor respuesta es **sacar la pregunta del servicio con event sourcing** y llevarla a un
  servicio con un almacenamiento que la conteste bien, como se hizo con los recordatorios.

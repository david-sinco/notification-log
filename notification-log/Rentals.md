# Servicio Rentals — referencia de diseño

> Especificación para implementar el servicio de publicación y consulta de apartamentos, desde el
> borrador hasta el cierre. **Nada de lo descrito aquí está implementado todavía.** Depende del servicio
> Identity (ver [`Person.md`](Person.md)) y del servicio Notification, que ya existe.

---

## 1. El negocio: "Llave"

**Llave** es un portal colombiano donde los propietarios publican apartamentos para arrendar o vender,
por su cuenta o con un asesor, y los interesados los encuentran, los visitan y hacen ofertas. La firma
del contrato ocurre fuera de la plataforma, pero Llave registra todo el proceso hasta el **cierre**: la
reserva, la firma y el valor final.

| Actor          | Qué puede hacer                                                           | Requisito en Identity            |
|---             |---                                                                        |---                               |
| **Visitante**  | buscar y ver publicaciones                                                | ninguno                          |
| **Interesado** | favoritos y búsquedas guardadas                                           | cuenta                           |
|                | consultas, visitas y reportes                                             | + teléfono verificado            |
|                | ofertas                                                                   | + documento verificado           |
| **Publicador** | publicar su apartamento                                                   | teléfono y documento verificados |
| **Asesor**     | publicar en nombre de un propietario o gestionar una publicación asignada | `Advisor` activo                 |
| **Moderador**  | aprobar, rechazar, suspender y rehabilitar publicaciones                  | permiso de moderador en el token |
| **Sistema**    | vencimientos, recordatorios y alertas                                     | —                                |

**Fuera de alcance:** pagos, firma electrónica, calificaciones entre usuarios, chat en tiempo real,
búsqueda por mapa y almacenamiento de fotos (el dominio solo guarda referencias a ellas).

Moneda única: pesos colombianos. Zona horaria de referencia para las franjas de visita: `America/Bogota`.

## 2. El flujo hasta el cierre

```
Publicador:  Borrador → EnRevisión → Publicada
Interesado:                           ├─ favorito / búsqueda guardada
                                      ├─ Consulta (mensajes)
                                      ├─ Visita: Solicitada → Confirmada → Realizada
                                      └─ Oferta: Abierta ⇄ Contraoferta → Aceptada
Publicación:                                                  └→ Reservada (10 días) → Cerrada
```

**Estados de la publicación:**

| Desde                  | Acción                                  | Hacia       | Quién                  |
|---                     |---                                      |---          |---                     |
| —                      | crear                                   | `Draft`     | publicador o asesor    |
| `Draft`                | enviar a revisión                       | `InReview`  | publicador o asesor    |
| `InReview`             | aprobar                                 | `Published` | moderador              |
| `InReview`             | rechazar, con motivos                   | `Draft`     | moderador              |
| `Published`            | editar los datos del inmueble           | `InReview`  | publicador o asesor    |
| `Published`            | pausar                                  | `Paused`    | publicador o asesor    |
| `Paused`               | reanudar                                | `Published` | publicador o asesor    |
| `Published`            | pasan 60 días                           | `Expired`   | sistema                |
| `Expired`              | renovar                                 | `Published` | publicador             |
| `Published`            | aceptar una oferta                      | `Reserved`  | publicador             |
| `Reserved`             | cancelar o vence la reserva             | `Published` | publicador o sistema   |
| `Reserved`             | cerrar                                  | `Closed`    | publicador             |
| `Published` / `Paused` | retirar                                 | `Withdrawn` | publicador o moderador |
| `Published` / `Paused` | tercer reporte o decisión del moderador | `Suspended` | sistema o moderador    |
| `Suspended`            | rehabilitar                             | `Published` | moderador              |

`Closed` y `Withdrawn` son estados finales.

## 3. Reglas de negocio

Cada regla tiene un código para nombrar los tests. Las marcadas como **(blanda)** dependen de datos de
muchos agregados: se validan en Application contra una proyección de apoyo y pueden fallar en una
carrera entre dos peticiones simultáneas. Es un riesgo aceptado.

### Publicación

- **RN-01** Solo publican personas con teléfono y documento verificados. Si publica un asesor, el
  propietario también debe tener el documento verificado.
- **RN-02** Un particular tiene como máximo 3 publicaciones activas (`InReview`, `Published`, `Paused` o
  `Reserved`). Un asesor, hasta la capacidad de su perfil. **(blanda)**
- **RN-03** Para enviar a revisión hacen falta:
  - tipo (apartamento o apartaestudio) y operación (venta o arriendo);
  - dirección completa, ciudad y barrio;
  - área de al menos 20 m²;
  - al menos 1 baño;
  - estrato entre 1 y 6;
  - administración de 0 o más;
  - descripción de 100 a 2.000 caracteres;
  - entre 5 y 30 fotos;
  - precio.
- **RN-04** Precio mínimo, para evitar errores de digitación: arriendo, $300.000 al mes; venta,
  $30.000.000.
- **RN-05** Si interviene un asesor, la ciudad de la publicación tiene que estar entre sus zonas de
  servicio.

### Moderación

- **RN-06** Toda publicación pasa por revisión. Para rechazar hay que indicar al menos un motivo de la
  lista: fotos de baja calidad, datos inconsistentes, precio sospechoso, dirección inválida o contenido
  prohibido.
- **RN-07** Editar las fotos, la descripción, el área, las habitaciones o la dirección de una publicación
  ya publicada la devuelve a revisión. Cambiar el precio o pausarla, no.

### Vigencia

- **RN-08** Una publicación vence 60 días después de su aprobación o de su última renovación. Se avisa al
  publicador 7 días antes.
- **RN-09** Se puede renovar si está publicada y le quedan 7 días o menos, o si ya venció. La renovación
  da 60 días más contados desde ese momento.

### Precio

- **RN-10** El precio se puede cambiar como máximo una vez cada 24 horas.
- **RN-11** El precio de una publicación reservada no se puede cambiar.
- **RN-12** Una bajada de al menos el 3% avisa a quienes la tienen en favoritos.

### Favoritos y búsquedas guardadas

- **RN-13** Máximo 100 favoritos por usuario. Solo se pueden añadir publicaciones publicadas; si después
  cambian de estado, siguen en la lista mostrando su estado actual.
- **RN-14** Máximo 10 búsquedas guardadas por usuario. Cada una tiene una frecuencia de alertas:
  inmediata, diaria (a las 7:00 en la hora del usuario) o sin alertas. Solo se envían alertas si el
  consentimiento `Alertas` está vigente en Identity.

### Consultas

- **RN-15** Hay una sola consulta por interesado y publicación. Si ya existe, un mensaje nuevo se añade a
  la misma conversación.
- **RN-16** Los mensajes tienen entre 1 y 1.000 caracteres y **no pueden contener teléfonos, correos ni
  enlaces**. Los datos de contacto de las dos partes solo se revelan cuando se reserva el inmueble.
- **RN-17** Máximo 10 consultas nuevas por interesado al día. **(blanda)**
- **RN-18** Se calcula y se muestra públicamente el tiempo de respuesta del publicador. Esta regla vive
  solo en el lado de lectura.

### Visitas

- **RN-19** Solo se pueden visitar publicaciones publicadas. El interesado propone entre 1 y 3 franjas
  de una hora, con al menos 2 horas y como mucho 14 días de antelación, entre las 7:00 y las 19:00 hora
  de Colombia.
- **RN-20** Un interesado tiene como máximo 3 visitas pendientes en toda la plataforma. **(blanda)**
- **RN-21** El anfitrión (el publicador o su asesor) confirma una de las franjas o rechaza la solicitud.
  Si no responde en 48 horas o antes de la primera franja, la solicitud vence.
- **RN-22** El anfitrión no puede confirmar dos visitas que se solapen. **(blanda)**
- **RN-23** La dirección exacta solo se muestra al interesado con una visita confirmada. El catálogo
  solo muestra el barrio.
- **RN-24** Cancelar con menos de 12 horas de antelación cuenta como cancelación tardía. Un interesado
  que acumula 2 cancelaciones tardías o inasistencias en 30 días no puede pedir visitas durante 15 días.
  **(blanda)**
- **RN-25** Después de la visita, el anfitrión la marca como realizada o como inasistencia. Si no la
  marca en 72 horas, se da por realizada.

### Ofertas

- **RN-26** Para hacer una oferta hacen falta el documento verificado y **al menos una visita realizada**
  a ese inmueble.
- **RN-27** Cada interesado tiene como máximo una oferta activa por publicación.
- **RN-28** La oferta tiene que ser al menos el 80% del precio publicado. En arriendo incluye además la
  fecha de inicio (entre 7 y 90 días después) y la duración (entre 6 y 36 meses).
- **RN-29** Negociación:
  - el publicador acepta, rechaza o contraoferta;
  - el interesado acepta la contraoferta, contraoferta de nuevo o retira su oferta;
  - hay como máximo 3 contraofertas en total por oferta;
  - cada movimiento le da 72 horas a la otra parte, y si no responde, la oferta vence.
- **RN-30** Aceptar una oferta reserva la publicación. Solo puede haber una oferta aceptada por
  publicación, y el resto de ofertas activas se rechazan automáticamente con el motivo "inmueble
  reservado".

### Reserva y cierre

- **RN-31** La reserva dura 10 días. El publicador puede ampliarla una sola vez, 5 días más.
- **RN-32** Si la reserva vence o una de las partes la cancela (indicando el motivo), la publicación
  vuelve a `Published` y la oferta aceptada pasa a `FellThrough`.
- **RN-33** Solo se puede cerrar una publicación reservada. Al cerrarla se registran el valor final y la
  fecha de firma, y el estado es final. Además:
  - se cancelan las visitas futuras;
  - se cierran las consultas;
  - se avisa a quienes la tenían en favoritos;
  - sale del catálogo.

### Reportes

- **RN-34** Cualquier usuario con teléfono verificado puede reportar una publicación, una sola vez,
  indicando el motivo: fraude, ya no disponible, datos falsos o contenido inapropiado. Con 3 reportes de
  usuarios distintos, la publicación se suspende automáticamente.
- **RN-35** El moderador rehabilita la publicación (vuelve a `Published` y se archivan los reportes) o la
  retira.

## 4. Modelo de dominio

### Por qué estos agregados

- **Ofertas, visitas y consultas son agregados aparte de la publicación.** Una publicación puede tener
  muchas de cada una, cada una con su propio ciclo de vida y escritas por personas distintas al mismo
  tiempo. Si todo viviera en el flujo de `Listing`, cada escritura competiría con las demás por el mismo
  flujo. `Listing` guarda solo lo que necesitan sus propias reglas: la oferta reservada y los reportes.
- **Favoritos y búsquedas guardadas son un agregado por usuario** (`FavoriteList` y `SavedSearchList`).
  Así, los límites de RN-13 y RN-14 son reglas de verdad dentro del agregado, y no reglas blandas.
- **Los nombres de los agregados no coinciden con el de su carpeta.** Una clase `Favorites` dentro del
  namespace `...Domain.Favorites` choca con el propio namespace al usarse desde fuera. Por eso las
  carpetas son `Favorites/` y `SavedSearches/`, y las clases `FavoriteList` y `SavedSearchList`.

### 4.1 `Listing` (el flujo es el `ListingId`)

**Datos:** `PublisherId` (el `PersonId` del propietario), `CreatedBy` (el `UserId` de quien la creó),
`AdvisorId` opcional, operación, `PropertyDetails`, `Location`, precio, descripción, fotos, estado,
`ExpiresAt`, `LastPriceChangeAt`, `ReservedOfferId`, `ReservedUntil`, `ReservationExtended` y los ids de
quienes la han reportado.

**Estados:** `Draft | InReview | Published | Paused | Expired | Reserved | Closed | Withdrawn |
Suspended`.

| Evento                                                                    | Cuándo                                                                                |
|---                                                                        |---                                                                                    |
| `ListingDrafted(ListingId, PublisherId, CreatedBy, AdvisorId, Operation)` | se crea                                                                               |
| `ListingDetailsUpdated(…estado completo…)`                                | se guarda el formulario de datos (detalles, ubicación, descripción)                   |
| `ListingPhotosUpdated(Photos)`                                            | se suben, reordenan o eliminan fotos                                                  |
| `ListingPriceChanged(OldPrice, NewPrice)`                                 | se fija o cambia el precio, también en borrador. El historial de precios sale de aquí |
| `ListingSubmittedForReview`                                               | se envía a revisión, o se edita estando publicada (RN-07)                             |
| `ListingApproved(ModeratorId, ExpiresAt)`                                 | el moderador aprueba                                                                  |
| `ListingRejected(ModeratorId, Reasons)`                                   | el moderador rechaza                                                                  |
| `ListingPaused` / `ListingResumed`                                        | se pausa o se reanuda                                                                 |
| `ListingRenewed(ExpiresAt)` / `ListingExpired`                            | se renueva o vence                                                                    |
| `AdvisorAssigned(AdvisorId)` / `AdvisorUnassigned`                        | se asigna o se quita el asesor                                                        |
| `ListingReserved(OfferId, ReservedUntil)`                                 | se acepta una oferta                                                                  |
| `ReservationExtended(ReservedUntil)`                                      | se amplía la reserva                                                                  |
| `ReservationReleased(Reason)`                                             | se cancela o vence la reserva                                                         |
| `ListingClosed(OfferId, FinalPrice, SignedOn)`                            | se cierra                                                                             |
| `ListingWithdrawn(Reason)`                                                | se retira                                                                             |
| `ListingReported(ReporterId, Reason)`                                     | un usuario la reporta. Al tercero se emite también `ListingSuspended` (RN-34)         |
| `ListingSuspended(Reason)` / `ListingReinstated(ModeratorId)`             | se suspende o se rehabilita                                                           |

### 4.2 `Offer` (el flujo es el `OfferId`)

**Datos:** `ListingId`, `OffererId`, operación, monto vigente, condiciones de arriendo (fecha de inicio y
meses), estado, número de contraofertas, quién movió por última vez y `RespondBy`.

**Estados:** `AwaitingPublisher ⇄ AwaitingOfferer → Accepted | Rejected | Withdrawn | Expired`, y de
`Accepted` puede pasar a `FellThrough`.

**Eventos:** `OfferSubmitted(OfferId, ListingId, OffererId, Amount, RentTerms, RespondBy)`,
`OfferCountered(By, Amount, RespondBy)`, `OfferAccepted(By)`, `OfferRejected(By, Reason)`,
`OfferWithdrawn`, `OfferExpired` y `OfferFellThrough(Reason)`.

### 4.3 `Visit` (el flujo es el `VisitId`)

**Estados:** `Requested → Confirmed → Completed | NoShow`. Desde `Requested` también puede pasar a
`Declined` o `Expired`, y desde `Confirmed` a `Cancelled`.

**Eventos:** `VisitRequested(VisitId, ListingId, VisitorId, HostId, Slots, RespondBy)`,
`VisitConfirmed(Slot)`, `VisitDeclined(Reason)`, `VisitRequestExpired`,
`VisitCancelled(By, Reason, IsLate)`, `VisitCompleted` y `VisitNoShow`.

### 4.4 `Inquiry` (el flujo es un id calculado a partir de la publicación y el interesado)

**Eventos:** `InquiryOpened(InquiryId, ListingId, SeekerId, Message)`,
`InquiryMessagePosted(AuthorId, Text)` e `InquiryClosed(Reason)`.

### 4.5 `FavoriteList` (el flujo es el `UserId`)

**Eventos:** `ListingFavorited(ListingId)` y `ListingUnfavorited(ListingId)`. Contiene el límite de 100,
y añadir o quitar dos veces lo mismo no genera evento.

### 4.6 `SavedSearchList` (el flujo es el `UserId`)

**Eventos:** `SavedSearchCreated(SearchId, Name, Criteria, Frequency)`,
`SavedSearchUpdated(SearchId, Name, Criteria, Frequency)` y `SavedSearchDeleted(SearchId)`. Contiene el
límite de 10. Cada búsqueda es una entidad hija, `SavedSearch`, igual que `NotificationConfiguration`
dentro de `NotificationTrigger`.

### 4.7 Value objects compartidos

`Money` (pesos colombianos, sin decimales), `Operation` (venta o arriendo), `Stratum` (1 a 6) y
`TimeSlot`, en `Shared/`.

### 4.8 Réplicas de Identity

No son agregados: son documentos locales alimentados por los eventos que publica Identity.

- verificación de cada persona (teléfono y documento);
- estado, zonas y capacidad de cada asesor;
- consentimiento de alertas de cada persona.

Application las usa para las reglas RN-01, RN-05, RN-14, RN-19 y RN-26.

## 5. Decisiones de diseño

- **Mismas convenciones que Identity** (ver `Person.md`, sección 4): event sourcing con Marten,
  `AggregateRoot` compartido, constructor privado con fábrica estática, eventos en pasado y solo con tipos
  primitivos, un tipo por archivo, namespace según la ruta, sin comentarios en el código, y el estándar
  Command/Handler/Validator.
- **El tiempo entra por parámetro.** Muchas reglas dependen de la hora (24 h, 60 días, 72 h). Los métodos
  del agregado reciben `DateTimeOffset now`, y Application lo obtiene de `TimeProvider`, que viene con
  .NET. Así los tests controlan el reloj sin trucos.
- **Aceptar una oferta modifica dos agregados en una transacción.** El handler `AcceptOffer` carga la
  oferta y la publicación, llama a `offer.Accept()` y `listing.Reserve(offerId)`, y guarda los dos flujos
  en la misma sesión de Marten. Es una excepción consciente a "un agregado por transacción": la regla de
  que solo haya una oferta aceptada (RN-30) está en `Listing`, y si la reserva falla, la aceptación
  también debe deshacerse.
- **El id de la consulta se calcula a partir del par.** Se genera un UUID v5 a partir de
  `ListingId + SeekerId`. "Una sola consulta por par" (RN-15) sale gratis: crear un flujo con un id que ya
  existe falla. El value object `InquiryMessage` rechaza los teléfonos, correos y enlaces (RN-16).
- **Las reglas blandas usan proyecciones de apoyo del lado de escritura.** Un conteo que sirve para
  decidir es un dato de escritura. Por eso los contadores (publicaciones activas por publicador, visitas
  pendientes por interesado, consultas del día, agenda del anfitrión…) son proyecciones inline de
  Infrastructure.Write, y no vistas del lado de lectura. Así Infrastructure.Write nunca depende de
  Application.Read.
- **Los mensajes programados son idempotentes.** Cada uno lleva el dato que lo originó (por ejemplo, el
  `RespondBy` o el `ExpiresAt`) y al ejecutarse comprueba que siga siendo el mismo. Si la oferta ya
  recibió una contraoferta, el vencimiento de la oferta original no hace nada.
- **El historial de precios no necesita modelo propio:** sale de los eventos `ListingPriceChanged`.
- **Las claves de evento para Notification** cumplen el formato de `EventKey`,
  `^[a-z0-9]+(\.[a-z0-9]+)+$`: minúsculas, números y puntos, **sin guiones bajos**. Viven como constantes
  en `NotificationKeys`.

## 6. Procesos automáticos

| Qué los dispara                       | Qué hacen                                                                               | Cómo                             |
|---                                    |---                                                                                      |---                               |
| `ListingApproved` / `ListingRenewed`  | programan el aviso 7 días antes y el vencimiento a los 60 días                          | mensaje programado de Wolverine  |
| `OfferSubmitted` / `OfferCountered`   | programan el vencimiento a las 72 h                                                     | mensaje programado               |
| `VisitRequested`                      | programa el vencimiento a las 48 h                                                      | mensaje programado               |
| `VisitConfirmed`                      | programa los recordatorios 24 h y 2 h antes, y dar la visita por realizada 72 h después | mensaje programado               |
| `ListingReserved`                     | rechaza las demás ofertas activas y programa el fin de la reserva a los 10 días         | suscripción + mensaje programado |
| `ListingClosed` / `ListingWithdrawn`  | rechaza ofertas, cancela visitas futuras, cierra consultas y avisa a favoritos          | suscripción                      |
| `ListingPriceChanged` con bajada ≥ 3% | avisa a favoritos                                                                       | suscripción                      |
| `ListingApproved` / bajada de precio  | cruza la publicación con las búsquedas guardadas con alertas inmediatas                 | suscripción asíncrona            |
| todos los días a las 7:00             | envía el resumen de las búsquedas guardadas con alertas diarias                         | tarea recurrente                 |
| cualquier evento con aviso asociado   | envía la notificación correspondiente (sección 8)                                       | suscripción                      |

## 7. Lado de lectura

| Vista                    | Para qué                                                                                                         | Proyección                       |
|---                       |---                                                                                                               |---                               |
| `ListingCatalogView`     | búsqueda pública con filtros, orden y paginación. Solo publicaciones publicadas y sin dirección exacta           | asíncrona: es la de más lecturas |
| `ListingDetailView`      | ficha pública: fotos, precio, historial de precio, número de favoritos, tiempo de respuesta y tarjeta del asesor | inline                           |
| `PublisherDashboardView` | mis publicaciones: estado, días para vencer, ofertas activas, visitas próximas y consultas sin responder         | inline, de varios flujos         |
| `SeekerActivityView`     | mis favoritos con su precio y estado actuales, mis visitas, ofertas y consultas                                  | inline, de varios flujos         |
| `InquiryThreadView`      | la conversación de una consulta, para las dos partes                                                             | inline                           |
| `HostCalendarView`       | la agenda de visitas del anfitrión                                                                               | inline                           |
| `ModerationQueueView`    | publicaciones pendientes de revisión y suspendidas                                                               | inline                           |
| `SavedSearchIndexView`   | búsquedas indexadas por criterios, para cruzarlas con publicaciones nuevas                                       | asíncrona                        |

Las vistas "de varios flujos" combinan eventos de `Listing`, `Offer`, `Visit` e `Inquiry`; en Marten son
proyecciones multi-flujo (`MultiStreamProjection`).

## 8. Integración con otros servicios

### Identity → Rentals

Identity publica los cambios de verificación, de asesores y de consentimientos. Rentals los consume y
mantiene sus réplicas. Esos contratos todavía no existen en `NotificationLog.Contracts`.

### Rentals → Notification

Rentals publica `NotificationDispatchRequested` con estas claves de evento:

| Área                  | Claves                                                                                                                                              |
|---                    |---                                                                                                                                                  |
| Publicación           | `publicacion.aprobada`, `publicacion.rechazada`, `publicacion.vence.pronto`, `publicacion.vencida`, `publicacion.suspendida`, `publicacion.cerrada` |
| Consultas             | `consulta.nueva`, `consulta.respuesta`                                                                                                              |
| Visitas               | `visita.solicitada`, `visita.confirmada`, `visita.rechazada`, `visita.cancelada`, `visita.recordatorio`                                             |
| Ofertas               | `oferta.recibida`, `oferta.contraoferta`, `oferta.aceptada`, `oferta.rechazada`, `oferta.vencida`                                                   |
| Reserva               | `reserva.liberada`                                                                                                                                  |
| Favoritos y búsquedas | `favorito.precio.baja`, `favorito.no.disponible`, `busqueda.coincidencia`, `busqueda.resumen`                                                       |

Notification solo envía a personas que ya tiene en su réplica de destinatarios, así que depende de que
Identity le publique antes los contactos. Por cada clave hay que crear en Notification su trigger y su
plantilla.

### Autenticación

La API valida los tokens JWT que emite Identity. Usa el permiso de asesor y el de moderador.

## 9. Estructura de proyectos

```
Services/Rentals/
├── NotificationLog.RentalService.Domain
├── NotificationLog.RentalService.Application.Write
├── NotificationLog.RentalService.Application.Read
├── NotificationLog.RentalService.Infrastructure.Write
├── NotificationLog.RentalService.Infrastructure.Read
├── NotificationLog.RentalService.Api
└── NotificationLog.RentalService.Tests
```

**Referencias entre proyectos:**

| Proyecto             | Referencia a                                                         | No puede referenciar |
|---                   |---                                                                   |---                   |
| Domain               | Domain.Shared                                                        | nada más             |
| Application.Write    | Domain, Application.Shared                                           | nada de lectura      |
| Application.Read     | Application.Shared, Domain (solo los eventos, para las proyecciones) | Application.Write    |
| Infrastructure.Write | Application.Write, Domain, NotificationLog.Contracts                 | nada de lectura      |
| Infrastructure.Read  | Application.Read, Domain (solo los eventos)                          | Application.Write    |
| Api                  | todos                                                                | —                    |

**Cambios en los proyectos compartidos:**

- `Application.Shared` recibe `PagedResult`, que hoy vive solo en Notification y pasará a usarse aquí.
- `NotificationLog.Contracts` necesita los contratos de Identity hacia Rentals.

## 10. Estructura de archivos

### 10.1 Domain

```
NotificationLog.RentalService.Domain/
├── Listings/
│   ├── Listing.cs
│   ├── IListingRepository.cs
│   ├── ListingStatus.cs                   Draft | InReview | Published | Paused | Expired |
│   │                                      Reserved | Closed | Withdrawn | Suspended
│   ├── PropertyType.cs                    Apartment | Studio
│   ├── RejectionReason.cs                 LowQualityPhotos | InconsistentData | SuspiciousPrice |
│   │                                      InvalidAddress | ProhibitedContent
│   ├── ReportReason.cs                    Fraud | NoLongerAvailable | FalseData | InappropriateContent
│   ├── ListingPolicy.cs                   plazos y límites: 60 días, 7 días, 24 h, 10+5 días, 3 reportes,
│   │                                      5–30 fotos, 100–2.000 caracteres, precios mínimos
│   ├── Events/
│   │   ├── ListingDrafted.cs
│   │   ├── ListingDetailsUpdated.cs
│   │   ├── ListingPhotosUpdated.cs
│   │   ├── ListingPriceChanged.cs
│   │   ├── ListingSubmittedForReview.cs
│   │   ├── ListingApproved.cs
│   │   ├── ListingRejected.cs
│   │   ├── ListingPaused.cs
│   │   ├── ListingResumed.cs
│   │   ├── ListingRenewed.cs
│   │   ├── ListingExpired.cs
│   │   ├── AdvisorAssigned.cs
│   │   ├── AdvisorUnassigned.cs
│   │   ├── ListingReserved.cs
│   │   ├── ReservationExtended.cs
│   │   ├── ReservationReleased.cs
│   │   ├── ListingClosed.cs
│   │   ├── ListingWithdrawn.cs
│   │   ├── ListingReported.cs
│   │   ├── ListingSuspended.cs
│   │   └── ListingReinstated.cs
│   └── ValueObjects/
│       ├── PropertyDetails.cs             tipo, área, habitaciones, baños, parqueaderos, estrato, piso,
│       │                                  ascensor, administración
│       ├── Location.cs                    ciudad, barrio, dirección
│       ├── ListingDescription.cs
│       └── Photo.cs                       referencia y orden
├── Offers/
│   ├── Offer.cs
│   ├── IOfferRepository.cs
│   ├── OfferStatus.cs                     AwaitingPublisher | AwaitingOfferer | Accepted | Rejected |
│   │                                      Withdrawn | Expired | FellThrough
│   ├── OfferParty.cs                      Publisher | Offerer
│   ├── Events/
│   │   ├── OfferSubmitted.cs
│   │   ├── OfferCountered.cs
│   │   ├── OfferAccepted.cs
│   │   ├── OfferRejected.cs
│   │   ├── OfferWithdrawn.cs
│   │   ├── OfferExpired.cs
│   │   └── OfferFellThrough.cs
│   └── ValueObjects/
│       └── RentTerms.cs                   fecha de inicio y meses
├── Visits/
│   ├── Visit.cs
│   ├── IVisitRepository.cs
│   ├── VisitStatus.cs                     Requested | Confirmed | Declined | Expired | Cancelled |
│   │                                      Completed | NoShow
│   ├── Events/
│   │   ├── VisitRequested.cs
│   │   ├── VisitConfirmed.cs
│   │   ├── VisitDeclined.cs
│   │   ├── VisitRequestExpired.cs
│   │   ├── VisitCancelled.cs
│   │   ├── VisitCompleted.cs
│   │   └── VisitNoShow.cs
│   └── ValueObjects/
│       └── ProposedSlots.cs               de 1 a 3 franjas válidas (RN-19)
├── Inquiries/
│   ├── Inquiry.cs
│   ├── IInquiryRepository.cs
│   ├── Events/
│   │   ├── InquiryOpened.cs
│   │   ├── InquiryMessagePosted.cs
│   │   └── InquiryClosed.cs
│   └── ValueObjects/
│       ├── InquiryId.cs                   UUID v5 a partir de ListingId + SeekerId
│       └── InquiryMessage.cs              1–1.000 caracteres, sin teléfonos, correos ni enlaces
├── Favorites/
│   ├── FavoriteList.cs
│   ├── IFavoriteListRepository.cs
│   └── Events/
│       ├── ListingFavorited.cs
│       └── ListingUnfavorited.cs
├── SavedSearches/
│   ├── SavedSearchList.cs
│   ├── SavedSearch.cs                     entidad hija
│   ├── ISavedSearchListRepository.cs
│   ├── AlertFrequency.cs                  Immediate | Daily | None
│   ├── Events/
│   │   ├── SavedSearchCreated.cs
│   │   ├── SavedSearchUpdated.cs
│   │   └── SavedSearchDeleted.cs
│   └── ValueObjects/
│       ├── SearchCriteria.cs              operación, ciudad, barrios, precio, habitaciones, estrato, área
│       └── PriceRange.cs
└── Shared/
    ├── Money.cs
    ├── Operation.cs                       Sale | Rent
    ├── Stratum.cs
    └── TimeSlot.cs
```

### 10.2 Application.Write

Cada carpeta de comando contiene `XCommand.cs`, `XHandler.cs` y `XValidator.cs`. Las marcadas con `*` no
llevan validador, como `SetTemplateStatus` en Notification. Las marcadas con **(sistema)** las dispara
un mensaje programado o una suscripción, nunca un usuario.

```
NotificationLog.RentalService.Application.Write/
├── Abstractions/
│   ├── IIdentityReplica.cs                verificación, asesor y consentimiento de alertas
│   ├── PersonVerification.cs
│   ├── AdvisorInfo.cs
│   ├── ISoftRuleChecks.cs                 conteos para las reglas blandas
│   ├── IProcessLookups.cs                 ofertas activas, visitas futuras y favoritos de una publicación
│   ├── ICommandScheduler.cs
│   └── INotificationDispatcher.cs
├── Listings/Commands/
│   ├── DraftListing/
│   ├── UpdateListingDetails/
│   ├── UpdateListingPhotos/
│   ├── ChangeListingPrice/
│   ├── SubmitListingForReview/ *
│   ├── ReviewListing/                     aprobar o rechazar
│   ├── SetListingAvailability/ *          pausar o reanudar
│   ├── RenewListing/ *
│   ├── AssignAdvisor/
│   ├── WithdrawListing/
│   ├── ReportListing/
│   ├── SuspendListing/
│   ├── ReinstateListing/ *
│   ├── ExtendReservation/ *
│   ├── CancelReservation/
│   ├── CloseListing/
│   ├── WarnListingExpiry/ *               (sistema)
│   ├── ExpireListing/ *                   (sistema)
│   └── ExpireReservation/ *               (sistema)
├── Offers/Commands/
│   ├── SubmitOffer/
│   ├── CounterOffer/
│   ├── AcceptOffer/ *                     modifica Offer y Listing en la misma sesión
│   ├── RejectOffer/
│   ├── WithdrawOffer/ *
│   └── ExpireOffer/ *                     (sistema)
├── Visits/Commands/
│   ├── RequestVisit/
│   ├── ConfirmVisit/
│   ├── DeclineVisit/
│   ├── CancelVisit/
│   ├── ReportVisitOutcome/                realizada o inasistencia
│   ├── ExpireVisitRequest/ *              (sistema)
│   ├── SendVisitReminder/ *               (sistema)
│   └── AutoCompleteVisit/ *               (sistema)
├── Inquiries/Commands/
│   ├── SendInquiryMessage/                abre la consulta o añade el mensaje
│   └── CloseInquiry/ *                    (sistema)
├── Favorites/Commands/
│   ├── AddFavorite/ *
│   └── RemoveFavorite/ *
├── SavedSearches/Commands/
│   ├── CreateSavedSearch/
│   ├── UpdateSavedSearch/
│   └── DeleteSavedSearch/ *
├── Processes/
│   ├── NotificationKeys.cs                constantes con las claves de evento de la sección 8
│   ├── ListingLifecycleProcess.cs         programa vencimientos; reacciona a reserva, cierre y retiro
│   ├── OfferProcess.cs                    programa los vencimientos de las ofertas
│   ├── VisitProcess.cs                    programa vencimientos, recordatorios y el autocompletado
│   ├── PriceDropProcess.cs                avisa a favoritos
│   ├── SavedSearchMatchingProcess.cs      cruce inmediato y resumen diario
│   └── ListingNotificationsProcess.cs     traduce eventos de dominio a NotificationDispatchRequested
└── DependencyInjection.cs
```

### 10.3 Application.Read

Cada carpeta de consulta contiene `XQuery.cs` y `XHandler.cs`. Los proyectores son funciones puras
(vista + evento → vista nueva); Infrastructure.Read se encarga de pasarles los eventos.

```
NotificationLog.RentalService.Application.Read/
├── Abstractions/
│   ├── IListingQueries.cs
│   ├── IPublisherQueries.cs
│   ├── ISeekerQueries.cs
│   ├── IInquiryQueries.cs
│   ├── IHostQueries.cs
│   └── IModerationQueries.cs
├── Listings/
│   ├── Queries/
│   │   ├── SearchCatalog/
│   │   └── GetListingDetail/
│   ├── Dtos/
│   │   ├── ListingCardDto.cs
│   │   ├── ListingDetailDto.cs
│   │   └── PricePointDto.cs
│   └── Projections/
│       ├── ListingCatalogView.cs
│       ├── ListingCatalogProjector.cs
│       ├── ListingDetailView.cs
│       └── ListingDetailProjector.cs
├── Publishers/
│   ├── Queries/
│   │   ├── GetMyListings/
│   │   └── GetListingOffers/
│   ├── Dtos/
│   │   ├── PublisherListingDto.cs
│   │   └── OfferSummaryDto.cs
│   └── Projections/
│       ├── PublisherDashboardView.cs
│       └── PublisherDashboardProjector.cs
├── Seekers/
│   ├── Queries/
│   │   ├── GetMyFavorites/
│   │   ├── GetMyVisits/
│   │   ├── GetMyOffers/
│   │   └── GetMySavedSearches/
│   ├── Dtos/
│   │   ├── FavoriteDto.cs
│   │   ├── VisitDto.cs
│   │   ├── OfferDto.cs
│   │   └── SavedSearchDto.cs
│   └── Projections/
│       ├── SeekerActivityView.cs
│       └── SeekerActivityProjector.cs
├── Inquiries/
│   ├── Queries/
│   │   ├── GetMyInquiries/
│   │   └── GetInquiryThread/
│   ├── Dtos/
│   │   ├── InquirySummaryDto.cs
│   │   └── InquiryThreadDto.cs
│   └── Projections/
│       ├── InquiryThreadView.cs
│       └── InquiryThreadProjector.cs
├── Hosts/
│   ├── Queries/
│   │   └── GetHostCalendar/
│   ├── Dtos/
│   │   └── CalendarEntryDto.cs
│   └── Projections/
│       ├── HostCalendarView.cs
│       └── HostCalendarProjector.cs
├── Moderation/
│   ├── Queries/
│   │   └── GetModerationQueue/
│   ├── Dtos/
│   │   └── ModerationItemDto.cs
│   └── Projections/
│       ├── ModerationQueueView.cs
│       └── ModerationQueueProjector.cs
├── SavedSearches/
│   └── Projections/
│       ├── SavedSearchIndexView.cs
│       └── SavedSearchIndexProjector.cs
└── DependencyInjection.cs
```

### 10.4 Infrastructure.Write

```
NotificationLog.RentalService.Infrastructure.Write/
├── Persistence/
│   ├── MartenStoreConfiguration.cs        tipos de evento y flujos
│   ├── MartenUnitOfWork.cs
│   ├── MartenListingRepository.cs
│   ├── MartenOfferRepository.cs
│   ├── MartenVisitRepository.cs
│   ├── MartenInquiryRepository.cs
│   ├── MartenFavoriteListRepository.cs
│   └── MartenSavedSearchListRepository.cs
├── DecisionProjections/                   proyecciones inline que alimentan las reglas blandas y los procesos
│   ├── PublisherCountersProjection.cs     publicaciones activas (RN-02)
│   ├── SeekerCountersProjection.cs        visitas pendientes, consultas del día, cancelaciones tardías,
│   │                                      visitas realizadas (RN-17, RN-20, RN-24, RN-26)
│   ├── HostScheduleProjection.cs          franjas confirmadas por anfitrión (RN-22)
│   ├── ListingEngagementProjection.cs     ofertas activas, visitas futuras y favoritos por publicación
│   ├── MartenSoftRuleChecks.cs
│   └── MartenProcessLookups.cs
├── IdentityReplica/
│   ├── PersonVerificationDocument.cs
│   ├── AdvisorDocument.cs
│   ├── AlertsConsentDocument.cs
│   └── MartenIdentityReplica.cs
├── Scheduling/
│   ├── WolverineCommandScheduler.cs
│   └── DailyDigestScheduler.cs            dispara el resumen diario de búsquedas guardadas
├── Messaging/
│   ├── RabbitMqMessagingExtensions.cs
│   ├── Consumers/                         eventos de Identity → réplicas
│   │   ├── PersonVerificationChangedHandler.cs
│   │   ├── AdvisorChangedHandler.cs
│   │   └── AlertsConsentChangedHandler.cs
│   ├── Subscriptions/
│   │   └── DomainEventsSubscription.cs    reparte los eventos de dominio entre los procesos de Application
│   └── Publishers/
│       └── WolverineNotificationDispatcher.cs   publica NotificationDispatchRequested
└── DependencyInjection.cs                 AddRentalsWrite()
```

### 10.5 Infrastructure.Read

```
NotificationLog.RentalService.Infrastructure.Read/
├── Projections/                           cada una delega en su proyector de Application.Read
│   ├── ListingCatalogProjection.cs
│   ├── ListingDetailProjection.cs
│   ├── PublisherDashboardProjection.cs    multi-flujo
│   ├── SeekerActivityProjection.cs        multi-flujo
│   ├── InquiryThreadProjection.cs
│   ├── HostCalendarProjection.cs
│   ├── ModerationQueueProjection.cs
│   └── SavedSearchIndexProjection.cs
├── Queries/
│   ├── MartenListingQueries.cs
│   ├── MartenPublisherQueries.cs
│   ├── MartenSeekerQueries.cs
│   ├── MartenInquiryQueries.cs
│   ├── MartenHostQueries.cs
│   └── MartenModerationQueries.cs
└── DependencyInjection.cs                 AddRentalsRead()
```

### 10.6 Api

```
NotificationLog.RentalService.Api/
├── Program.cs                             valida el JWT de Identity; llama a AddRentalsWrite() y AddRentalsRead()
├── appsettings.json
├── Properties/launchSettings.json
├── Endpoints/
│   ├── CatalogEndpoints.cs                búsqueda y ficha públicas
│   ├── ListingEndpoints.cs                gestión del publicador y del asesor
│   ├── ModerationEndpoints.cs
│   ├── OfferEndpoints.cs
│   ├── VisitEndpoints.cs
│   ├── InquiryEndpoints.cs
│   ├── FavoriteEndpoints.cs
│   └── SavedSearchEndpoints.cs
├── Contracts/
│   ├── Catalog/                           un request/response por endpoint, como en Notification
│   ├── Listings/
│   ├── Moderation/
│   ├── Offers/
│   ├── Visits/
│   ├── Inquiries/
│   ├── Favorites/
│   └── SavedSearches/
└── Exceptions/
    └── GlobalExceptionHandler.cs          AppValidationException → 400, NotFoundException → 404,
                                           DomainException → 422, cualquier otra → 500
```

### 10.7 Tests

```
NotificationLog.RentalService.Tests/
├── Domain/
│   ├── ListingTests.cs
│   ├── OfferTests.cs
│   ├── VisitTests.cs
│   ├── InquiryTests.cs
│   ├── InquiryMessageTests.cs
│   ├── FavoriteListTests.cs
│   └── SavedSearchListTests.cs
└── Application/
    ├── Listings/                          handlers con dobles en memoria y TimeProvider falso
    ├── Offers/
    ├── Visits/
    ├── Inquiries/
    └── Processes/                         idempotencia de los mensajes programados
```

## 11. Plan de implementación

| Fase                          | Qué construir                                                                                                                                               | Reglas                      | Cuándo está terminada                                                                                |
|---                            |---                                                                                                                                                          |---                          |---                                                                                                   |
| **1. Publicar**               | `Listing` desde `Draft` hasta `Published`, rechazo, pausa, retiro y precio; réplica de verificación de Identity; `ListingCatalogView` y `ListingDetailView` | RN-01 a RN-07, RN-10, RN-11 | se puede publicar un apartamento, aparece en el catálogo y el historial de precios se ve en la ficha |
| **2. Vigencia y favoritos**   | vencimiento y renovación programados, `FavoriteList` y aviso de bajada de precio                                                                            | RN-08, RN-09, RN-12, RN-13  | las publicaciones vencen solas y los favoritos reciben el aviso de bajada                            |
| **3. Consultas y visitas**    | `Inquiry` y `Visit` con sus recordatorios; proyecciones de apoyo de interesados y anfitriones                                                               | RN-15 a RN-25               | un interesado pregunta, agenda, recibe el recordatorio y el anfitrión marca el resultado             |
| **4. Ofertas y cierre**       | `Offer`, negociación, reserva, liberación y cierre, con sus procesos                                                                                        | RN-26 a RN-33               | el flujo completo, de la oferta al cierre, y la liberación de la reserva cuando vence                |
| **5. Búsquedas y moderación** | `SavedSearchList`, cruce con publicaciones, resumen diario y reportes                                                                                       | RN-14, RN-34, RN-35         | las alertas llegan con la frecuencia elegida y el tercer reporte suspende la publicación             |

## 12. Casos de prueba clave

| Dado                                                   | Cuando                                                | Entonces                                                                          |
|---                                                     |---                                                    |---                                                                                |
| publicación publicada con el precio cambiado hace 10 h | se cambia el precio otra vez                          | `DomainException` (RN-10)                                                         |
| publicación publicada con 2 reportes                   | un tercer usuario distinto la reporta                 | se emiten `ListingReported` y `ListingSuspended`                                  |
| arriendo publicado a $2.000.000                        | llega una oferta de $1.500.000                        | se rechaza al crearla: es el 75%, por debajo del 80% (RN-28)                      |
| publicación reservada                                  | se acepta otra oferta                                 | falla, y la aceptación no se guarda (RN-30)                                       |
| reserva de hace 10 días sin ampliar                    | se ejecuta el vencimiento programado                  | `ReservationReleased` + `OfferFellThrough`, y la publicación vuelve a `Published` |
| mensaje "llámame al 3001234567"                        | se envía por una consulta                             | se rechaza (RN-16)                                                                |
| oferta que ya recibió una contraoferta                 | llega el vencimiento programado de la oferta original | no hace nada (idempotencia)                                                       |
| interesado sin ninguna visita realizada                | hace una oferta                                       | se rechaza (RN-26)                                                                |
| publicación publicada editada en su descripción        | se guarda el cambio                                   | vuelve a `InReview` (RN-07)                                                       |
| publicación a la que le quedan 20 días                 | se intenta renovar                                    | se rechaza: solo se renueva con 7 días o menos, o ya vencida (RN-09)              |

## 13. Pendientes y dependencias

- **Identity:** añadir el permiso de moderador y publicar los eventos de verificación, de asesores y de
  consentimientos, con sus contratos en `NotificationLog.Contracts`.
- **Notification:** crear el trigger y la plantilla de cada clave de evento de la sección 8. Sigue abierto
  cómo enviar a direcciones todavía no verificadas (ver `Person.md`, sección 10).
- **Fotos:** falta decidir dónde se almacenan (almacenamiento de archivos) y cómo se suben. El dominio
  solo guarda la referencia y el orden de cada foto.
- **Mover `PagedResult`** de Notification a `Application.Shared`.
- **AppHost:** añadir la API de Rentals. PostgreSQL con Marten ya lo necesita Identity.

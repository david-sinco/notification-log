# Datos de la publicación (Listing)

Revisé `Listing`, `PropertyDetails`, `Location`, `ListingPolicy`, `ListingFilter`, `ListingView`, `MartenListingReadModel` y `CatalogEndpoints`, y crucé todo con `todo.md`. No modifiqué código.

## Lo que hay hoy

| Grupo | Datos |
|---|---|
| Operación | `Operation` (venta o arriendo), `Price` |
| Inmueble (`PropertyDetails`) | tipo (apartamento o apartaestudio), área, habitaciones, baños, parqueaderos, estrato, piso, ascensor, administración |
| Ubicación (`Location`) | ciudad, barrio y dirección, los tres en texto libre |
| Contenido | descripción (100 a 2.000 caracteres), de 5 a 30 fotos |
| Filtro del catálogo | `search` (busca dentro de ciudad, barrio y dirección), `operation` y paginación, ordenado por `UpdatedAt` |

---

## 1. Datos que no existen y que debería tener una publicación real

**Esenciales:** sin estos, la publicación no compite con un portal real.

| Dato | Dónde | Por qué |
|---|---|---|
| **Título** | `ListingTitle` (VO nuevo) | Hoy la tarjeta se arma con tipo + barrio, y todas se ven iguales. |
| **Área privada y área construida** | `PropertyDetails` | En Colombia se publican las dos. Hoy `Area` es una sola y no dice cuál es. |
| **Antigüedad** (año o rango: para estrenar, 1–8, 9–15, 16–30, más de 30) | `PropertyDetails` | Es uno de los primeros datos que mira quien compra. |
| **Coordenadas** (lat/lng) | `Location`, con un VO `GeoPoint` | Para el mapa y para buscar por zona dibujada. |
| **Localidad, comuna o zona** | `Location` | El barrio es demasiado fino y la ciudad demasiado gruesa (las localidades de Bogotá, las comunas de Medellín). |
| **Amoblado** | `PropertyDetails` | Es decisivo en arriendo. |
| **Disponible desde** (solo arriendo) | `RentalTerms` (VO nuevo) | Quien busca arriendo filtra por fecha de mudanza. |
| **Mascotas permitidas** (solo arriendo) | `RentalTerms` | Es un filtro muy usado. |
| **Administración incluida en el canon** (solo arriendo) | `RentalTerms` | Sin este dato no se puede calcular el costo mensual real. |

**Recomendables:**

| Dato | Dónde |
|---|---|
| Características del inmueble (balcón, terraza, estudio, cuarto útil o depósito, chimenea, cocina abierta o integral, vista exterior, calentador a gas), como lista de enum | `PropertyFeatures` |
| Amenidades del conjunto (conjunto cerrado, portería o vigilancia 24 h, piscina, gimnasio, BBQ, salón comunal, zona infantil, parqueadero de visitantes, zonas verdes), como lista de enum | `BuildingAmenities` |
| Tipo de parqueadero (cubierto o descubierto, privado o comunal) | `PropertyDetails` |
| Pisos del edificio (para mostrar algo como «piso 8 de 12») | `PropertyDetails` |
| Requisitos de arriendo (codeudor, póliza o aseguradora, depósito) y duración mínima del contrato | `RentalTerms` |
| Precio negociable; acepta crédito hipotecario o leasing (venta) | `SaleTerms` (VO nuevo) |
| Nombre del conjunto o edificio | `Location` |

**Opcionales:**
- URL de video o de tour virtual.
- Plano.
- Medios baños.
- Matrícula inmobiliaria: solo la vería el moderador, como dato de verificación, nunca en público.

---

## 2. Qué debería ir en el filtro del catálogo

Este cuadro considera todos los datos, los de hoy y los nuevos.

| Filtro | Tipo | Prioridad | Nota |
|---|---|---|---|
| Operación | único | ya existe | |
| Ciudad | exacto, desde un catálogo | **alta** | Hoy es `Contains` sobre texto libre. |
| Localidad o barrio | multiselección | **alta** | |
| Precio | rango mín–máx | **alta** | |
| Habitaciones | mínimo (1, 2, 3, 4+) | **alta** | |
| Baños | mínimo | **alta** | |
| Área | rango | **alta** | Sobre el área privada. |
| Estrato | multiselección 1–6 | **alta** | Es muy colombiano y muy usado. |
| Tipo de inmueble | multiselección | media | Hoy solo hay dos tipos. |
| Parqueaderos | mínimo | media | |
| Antigüedad | rango | media | |
| Amoblado / mascotas | booleanos | media | Solo aplican en arriendo. |
| Disponible desde | fecha ≤ | media | Solo arriendo. |
| Costo mensual total (canon + administración) | rango | media | Se calcula en la vista. Reemplaza filtrar la administración por separado. |
| Ascensor | booleano | baja | |
| Amenidades | multiselección («tiene todas») | baja | |
| Publicadas en los últimos N días | | baja | |
| Zona del mapa (bounding box) | | baja | Requiere las coordenadas. |

**No deberían ser filtros:**
- Dirección. Además, hoy `search` busca dentro de ella en el catálogo público, lo que se cruza con el pendiente de qué datos se muestran sin iniciar sesión.
- Descripción y título: entran por `search`.
- Piso exacto.
- Administración sola.

**Ordenamiento** (hoy no existe): más recientes, por precio (ascendente o descendente), por área y por precio por m².

---

## 3. Qué le falta a Listing en cuanto a datos

1. **Ciudad y barrio son texto libre.** «Bogotá», «bogota» y «Bogotá D.C.» cuentan como valores distintos, así que ningún filtro exacto funciona. `Contains` con `OrdinalIgnoreCase` tampoco ignora las tildes. Recomiendo un catálogo de ciudades con código DIVIPOLA (DANE) y barrios normalizados, o al menos normalizar sin tildes en la proyección.
2. **Faltan validaciones entre campos.** Un `Studio` acepta 3 habitaciones y un `Apartment` acepta 0. El área privada debería ser menor o igual que la construida (cuando exista). `Floor` es opcional aunque el tipo siempre es apartamento.
3. **«Cero» y «no informado» se ven igual.** `AdministrationFee` es obligatorio y puede ser 0, así que no se distingue «no paga administración» de «no lo dijo». `HasElevator` es `bool`, así que pasa lo mismo.
4. **Falta `PublishedAt`.** El catálogo ordena por `UpdatedAt`, que cambia con cualquier edición, así que editar una publicación la sube al primer lugar. Hace falta la fecha de la primera aprobación (o de la última renovación) en `ListingView`.
5. **`ListingSummaryDto` es pobre para una tarjeta de catálogo.** Le faltan baños, parqueaderos, estrato, administración y título.
6. **El catálogo devuelve `ListingDto` completo**, con la dirección y `OwnerId` (ya está en `todo.md`). Cuando lleguen las coordenadas, en público habría que exponerlas aproximadas.
7. **`ListingFilter` mezcla dos usos.** `Status` y `ParticipantId` son de administración. Conviene un `CatalogCriteria` aparte, que además sirve tal cual para las **búsquedas guardadas** de `todo.md`, cuyos criterios son los mismos.
8. **Faltan índices en Marten.** Hoy no hay ningún índice calculado sobre `ListingView`. Con más filtros harían falta, al menos sobre `Status`, `Operation`, `City`, `Price`, `Bedrooms` y `Stratum`.

---

## Cómo implementarlo (lo que hay que decidir)

- **Eventos nuevos o ampliar `ListingDetailsUpdated`.** Recomiendo **eventos separados por sección**: `ListingTitleChanged`, `ListingFeaturesUpdated`, `ListingRentalTermsUpdated` y `ListingLocated` (coordenadas). Ampliar el evento actual obliga a que los campos nuevos sean nulos en los flujos viejos, y entonces «nulo» queda ambiguo: no se sabe si es «no existía el campo» o «no lo informó». Las secciones separadas también encajan con un formulario por pasos.
- **Qué vuelve a revisión.** Hoy cualquier `UpdateDetails` sobre una publicación visible la manda a revisión. Si cambiar «disponible desde» o «mascotas» también la saca del catálogo, la publicación se vuelve muy rígida. Propongo que solo vuelvan a revisión el título, el inmueble, la ubicación, la descripción y las fotos, y que los términos no.
- **Publicaciones ya publicadas.** Si el título y las coordenadas pasan a ser obligatorios en `SubmitForReview`, hay que decidir qué pasa con las que ya están publicadas. Pueden quedar exentas hasta su próxima edición, o se exige completar los datos al renovar.
- **`RentalTerms` y `SaleTerms`** dependen de `Operation`. El agregado debe rechazar términos que no correspondan a la operación de la publicación.

Un orden posible:
1. Normalizar la ubicación, agregar `PublishedAt` y el ordenamiento, y separar `CatalogCriteria` con los filtros de prioridad alta.
2. Título, áreas, antigüedad y `RentalTerms`.
3. Características y amenidades.
4. Coordenadas y mapa.

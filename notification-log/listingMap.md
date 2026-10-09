# Mapa de publicaciones

Revisé `Location`, `ListingView`, `CatalogEndpoints`, la ficha del portal (`UI/portal/app/publicaciones/[id]`) y el editor de la Web (`ListingDetail.razor`). Hoy ninguna publicación tiene coordenadas: la ubicación es ciudad, barrio y dirección en texto libre. No modifiqué código.

Complementa la fase 4 de `propertyDetail.md` (coordenadas y búsqueda por mapa).

## Leaflet sí, pero Leaflet no es el mapa

Leaflet es gratis y es buena elección, pero es solo la librería que **dibuja**. Un mapa necesita además dos servicios externos, y ahí está el costo real:

| Pieza | Qué hace | Opción gratis | Cuidado |
|---|---|---|---|
| **Librería** | Dibuja el mapa, los marcadores y el zoom | Leaflet (en el portal, `react-leaflet`) | Ninguno. |
| **Teselas (tiles)** | Las imágenes del mapa | `tile.openstreetmap.org` | Su política de uso es para tráfico bajo: sirve para desarrollo y para un proyecto de enseñanza, pero no para producción. Exige mostrar «© OpenStreetMap contributors». Para producción: un proveedor con capa gratuita (MapTiler, Stadia, Carto) con API key. |
| **Geocodificación** | Convierte una dirección en coordenadas | Nominatim (OSM) | Máximo 1 petición por segundo y prohíbe el autocompletado. Además, entiende mal las direcciones colombianas («Cra 7 # 72-41»). |

Recomendaciones:
- **La URL de las teselas y la atribución van en configuración**, no en el código. Así se cambia de OSM a otro proveedor sin tocar componentes.
- **No depender de la geocodificación.** Que el propietario ubique el pin en el mapa. A lo sumo se geocodifica la ciudad y el barrio para centrar el mapa al abrirlo, que Nominatim sí resuelve bien.

Google Maps también tiene una cuota gratuita mensual, pero exige tarjeta y API key, y sus condiciones obligan a usar sus teselas. Leaflet + OSM no tiene ninguna de esas ataduras.

---

## Dónde aparece el mapa

| Lugar | Para qué | Precisión |
|---|---|---|
| **Web: editor de la publicación** | El propietario o el moderador ubica el pin arrastrándolo | Exacta |
| **Web: moderación** | Verificar que el pin coincide con la dirección (motivo de rechazo `InvalidAddress`) | Exacta |
| **Portal: ficha de la publicación** | Mostrar la zona | **Aproximada** (círculo) |
| **Portal: catálogo** | Vista de mapa con los resultados y búsqueda por zona visible | **Aproximada** |
| **Portal: visita agendada** (opcional) | Mostrar la ubicación exacta solo a quien ya tiene la visita confirmada | Exacta |

---

## Privacidad: la coordenada exacta nunca sale al catálogo

Un pin exacto en el catálogo público es lo mismo que publicar la dirección, y eso ya está pendiente de decidir en `todo.md` («qué datos se muestran sin iniciar sesión»).

- La **proyección** guarda dos pares: el exacto y el aproximado. El catálogo solo devuelve el aproximado.
- El aproximado se calcula **en el servidor**, con un desplazamiento de unos 200–300 m **determinista** (derivado del id de la publicación). Si fuera aleatorio en cada consulta, promediando varias respuestas se recuperaría el punto real.
- El portal dibuja un **círculo**, no un pin, para que se entienda que es una zona.

---

## Dominio (Rentals)

- **`GeoPoint`** (VO nuevo, en `Listings/ValueObjects`): latitud y longitud `decimal`, con validación de rango y, de forma opcional, que caiga dentro del rectángulo aproximado de Colombia (incluyendo San Andrés).
- **Evento propio `ListingLocated(Latitude, Longitude)`** y comando `LocateListing`, en vez de ampliar `ListingDetailsUpdated` (el porqué está en `propertyDetail.md`, «Cómo implementarlo»).
- **Decidir:**
  - Si el pin es obligatorio en `SubmitForReview`, y qué pasa con las publicaciones ya publicadas que no lo tienen.
  - Si mover el pin de una publicación visible la manda a revisión. La recomendación es que sí, igual que cambiar la ubicación.
  - Si al cambiar la ciudad o la dirección el pin anterior se descarta o se conserva.

---

## Lectura y API

- **`ListingView`**: `Latitude`, `Longitude`, `ApproxLatitude` y `ApproxLongitude`, todos nulos mientras no haya pin.
- **DTOs**: la ficha del catálogo devuelve solo el aproximado; `GET /api/listings/{id}` (staff y propietario) devuelve el exacto.
- **Filtro por zona**: `minLat`, `maxLat`, `minLng` y `maxLng` en `CatalogCriteria`, como rango sobre los campos aproximados, con índices calculados de Marten. PostGIS no hace falta a esta escala.
- **Endpoint de marcadores**: `GET /api/catalog/map` devuelve una lista liviana (id, coordenada aproximada, precio, tipo) para la zona visible, sin paginar y con tope (por ejemplo 500). La lista paginada de tarjetas no sirve para el mapa, porque solo trae una página.

---

## Web (Blazor Server)

- No hay un wrapper de Leaflet para Blazor bien mantenido. Lo recomendable es un **módulo JS propio y pequeño** en `wwwroot/lib`, cargado con `IJSObjectReference`: crear el mapa, poner el pin y avisar a .NET con `DotNetObjectReference` cuando el pin se suelta (`dragend`).
- Un componente `LocationPicker` en `Components/Rentals`, al lado de `ListingPhotosEditor`, usado en `ListingDetail.razor`.
- Liberar el mapa en `DisposeAsync`. Con render interactivo de servidor, el componente se destruye y se vuelve a crear al navegar.
- Leaflet y su CSS se sirven desde `wwwroot/lib`, igual que las demás librerías de la Web.

---

## Portal (Next.js)

- `react-leaflet` en un componente `"use client"` cargado con `dynamic(..., { ssr: false })`, porque Leaflet usa `window` y rompe el render del servidor.
- **Problema conocido:** el ícono del marcador por defecto de Leaflet no carga con los bundlers. Hay que importar las imágenes y configurar el ícono a mano, o usar un `divIcon` con la marca de Llave.
- Para muchos resultados en el catálogo: agrupación de marcadores (`leaflet.markercluster`).
- La URL de las teselas y la atribución en variables `NEXT_PUBLIC_…`.

---

## Orden posible

1. **Dominio y lectura**: `GeoPoint`, `ListingLocated`, `LocateListing`, la proyección con el par exacto y el aproximado, y los DTOs que separan uno del otro.
2. **Web**: `LocationPicker` en el editor, centrado con la ciudad y el barrio.
3. **Portal, ficha**: mapa con el círculo aproximado.
4. **Portal, catálogo**: filtro por zona, endpoint de marcadores, vista de mapa y agrupación.
5. **Opcional**: ubicación exacta en la visita agendada, y migrar las teselas a un proveedor con API key si el proyecto sale del entorno de desarrollo.

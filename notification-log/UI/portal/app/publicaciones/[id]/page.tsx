import Link from "next/link";
import { notFound } from "next/navigation";
import { auth } from "@/auth";
import { LoginButton } from "@/components/login-button";
import { money, operationLabel, place, propertyTypeLabel } from "@/lib/format";
import { ApiError, getCatalogListing, getMyVisitor } from "@/lib/rentals";
import { VisitRequestForm } from "./visit-request-form";

async function load(id: string) {
  try {
    return await getCatalogListing(id);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }
}

export default async function ListingPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const listing = await load(id);
  const session = await auth();
  const visitor = session ? await getMyVisitor() : null;

  const specs = [
    ["Tipo", propertyTypeLabel(listing.type)],
    ["Área", listing.area !== null ? `${listing.area} m²` : null],
    ["Habitaciones", listing.bedrooms],
    ["Baños", listing.bathrooms],
    ["Parqueaderos", listing.parkingSpots],
    ["Estrato", listing.stratum],
    ["Piso", listing.floor],
    ["Ascensor", listing.hasElevator ? "Sí" : "No"],
  ].filter(([, value]) => value !== null);

  return (
    <main className="main">
      <div className="container">
        <Link className="back-link" href="/">
          ← Volver a la búsqueda
        </Link>

        <header className="page-header">
          <p className="eyebrow">
            {operationLabel(listing.operation)} · {propertyTypeLabel(listing.type)}
          </p>
          <h1>{place(listing.neighborhood, listing.city)}</h1>
          {listing.address && <p>{listing.address}</p>}
        </header>

        {listing.photos.length > 0 ? (
          <div className="gallery">
            {listing.photos.map((photo) => (
              <img key={photo.fileName} src={photo.url} alt="" />
            ))}
          </div>
        ) : (
          <div className="listing-cover gallery-empty">
            <img src="/llave-mark.svg" alt="" />
          </div>
        )}

        <div className="detail-layout">
          <div className="stack">
            <section className="surface-card">
              <h2 className="section-title">Características</h2>
              <dl className="spec-grid">
                {specs.map(([label, value]) => (
                  <div className="spec" key={label}>
                    <dt>{label}</dt>
                    <dd>{value}</dd>
                  </div>
                ))}
              </dl>
            </section>

            {listing.description && (
              <section className="surface-card">
                <h2 className="section-title">Descripción</h2>
                <p className="description">{listing.description}</p>
              </section>
            )}
          </div>

          <aside className="detail-aside surface-card stack">
            <div>
              <p className="eyebrow">{operationLabel(listing.operation)}</p>
              <p className="listing-price">{money(listing.price)}</p>
              {listing.administrationFee !== null && (
                <p className="form-hint">Administración: {money(listing.administrationFee)}</p>
              )}
            </div>

            {!session ? (
              <>
                <p className="form-hint">Inicia sesión o crea tu cuenta para pedir una visita.</p>
                <LoginButton className="btn btn-primary btn-block">Iniciar sesión para visitar</LoginButton>
              </>
            ) : visitor?.status !== "Registered" ? (
              <>
                <p className="form-hint">Antes de pedir una visita necesitamos tus datos.</p>
                <Link className="btn btn-primary btn-block" href={`/perfil?next=/publicaciones/${listing.id}`}>
                  Completar mi perfil
                </Link>
              </>
            ) : (
              <VisitRequestForm listingId={listing.id} />
            )}
          </aside>
        </div>
      </div>
    </main>
  );
}

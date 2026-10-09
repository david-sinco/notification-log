import Link from "next/link";
import { notFound } from "next/navigation";
import { auth } from "@/auth";
import { LoginButton } from "@/components/login-button";
import { initials, listingTitle, longDate, money, operationLabel, propertyTypeLabel } from "@/lib/format";
import { ApiError, getCatalogListing } from "@/lib/rentals";
import { Gallery } from "./gallery";
import { VisitRequestForm } from "./visit-request-form";

async function load(id: string) {
  try {
    return await getCatalogListing(id);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }
}

export async function generateMetadata({ params }: { params: Promise<{ id: string }> }) {
  const listing = await load((await params).id);
  return { title: listingTitle(listing.type, listing.neighborhood) };
}

export default async function ListingPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const listing = await load(id);
  const session = await auth();
  const rent = listing.operation === "Rent";
  const ownListing = !!session && listing.ownerUserId === session.user.id;

  const specs = [
    ["Área", listing.area !== null ? `${listing.area} m²` : null],
    ["Habitaciones", listing.bedrooms],
    ["Baños", listing.bathrooms],
    ["Parqueaderos", listing.parkingSpots],
    ["Estrato", listing.stratum],
    ["Piso", listing.floor],
    ["Ascensor", listing.hasElevator ? "Sí" : "No"],
    ["Administración", listing.administrationFee !== null ? money(listing.administrationFee) : null],
  ].filter(([, value]) => value !== null);

  return (
    <main className="main" style={{ paddingTop: 24 }}>
      <div className="container stack" style={{ "--gap": "20px" } as React.CSSProperties}>
        <Link className="back-link" href="/">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M15 18l-6-6 6-6" />
          </svg>
          Volver a resultados
        </Link>

        <div className="row row-between row-end" style={{ "--gap": "12px" } as React.CSSProperties}>
          <div className="stack" style={{ "--gap": "8px" } as React.CSSProperties}>
            <div className="row" style={{ "--gap": "6px" } as React.CSSProperties}>
              <span className={`pill ${rent ? "pill-lime" : "pill-ink"}`}>{operationLabel(listing.operation)}</span>
              <span className="pill">{propertyTypeLabel(listing.type)}</span>
            </div>
            <h1 className="title">{listingTitle(listing.type, listing.neighborhood)}</h1>
            <p className="muted">{[listing.address, listing.neighborhood, listing.city].filter(Boolean).join(" · ")}</p>
          </div>
          {listing.expiresAt && <p className="eyebrow">Vigente hasta {longDate(listing.expiresAt)}</p>}
        </div>

        <Gallery photos={listing.photos} />

        <div className="split">
          <div className="split-main">
            <section className="card">
              <h2 className="h2">El inmueble</h2>
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
              <section className="card" style={{ "--gap": "12px" } as React.CSSProperties}>
                <h2 className="h2">Descripción</h2>
                <p className="description">{listing.description}</p>
              </section>
            )}

            {listing.ownerName && (
              <section className="card" style={{ flexDirection: "row", flexWrap: "wrap", alignItems: "center", gap: 14 }}>
                <span className="avatar avatar-lg" aria-hidden="true">
                  {initials(listing.ownerName)}
                </span>
                <div style={{ flex: "1 1 200px", minWidth: 0 }}>
                  <p className="eyebrow">Publica</p>
                  <p style={{ fontWeight: 600 }}>{listing.ownerName}</p>
                </div>
                <p className="small muted" style={{ flex: "1 1 260px" }}>
                  Coordinas la visita con el propietario aquí mismo. Sus datos de contacto no se muestran en el portal.
                </p>
              </section>
            )}
          </div>

          <aside className="split-aside">
            <section className="card card-dark" style={{ "--gap": "6px" } as React.CSSProperties}>
              <p className="eyebrow">{rent ? "Canon mensual" : "Precio de venta"}</p>
              <p className="price">{money(listing.price)}</p>
              {listing.administrationFee !== null && (
                <p className="small muted">+ {money(listing.administrationFee)} de administración</p>
              )}
            </section>

            <section className="card">
              {!session ? (
                <>
                  <h2 className="h2">¿Quieres conocerlo?</h2>
                  <p className="muted">
                    Ingresa con tu correo o celular para proponerle al propietario hasta tres horarios de visita.
                  </p>
                  <LoginButton className="btn btn-ink btn-block">Ingresar para pedir visita</LoginButton>
                </>
              ) : ownListing ? (
                <>
                  <h2 className="h2">Es tu publicación</h2>
                  <p className="muted">Las visitas a tus inmuebles se gestionan desde el backoffice de propietarios.</p>
                </>
              ) : (
                <VisitRequestForm listingId={listing.id} now={Date.now()} />
              )}
            </section>
          </aside>
        </div>
      </div>
    </main>
  );
}

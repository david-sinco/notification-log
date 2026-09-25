import Link from "next/link";
import { redirect } from "next/navigation";
import { auth } from "@/auth";
import { Pager } from "@/components/pager";
import { SignInRequired } from "@/components/sign-in-required";
import { dateTime, place, visitStatus } from "@/lib/format";
import { getListing, getMyVisitor, listMyVisits } from "@/lib/rentals";
import { CancelVisitForm } from "./cancel-visit-form";

export const metadata = { title: "Mis visitas" };

const cancellable = ["Requested", "Confirmed"];

export default async function VisitsPage({ searchParams }: { searchParams: Promise<{ pagina?: string }> }) {
  const session = await auth();
  if (!session) return <SignInRequired message="Inicia sesión para ver las visitas que has pedido." />;

  const visitor = await getMyVisitor();
  if (visitor?.status !== "Registered") redirect("/perfil?next=/visitas");

  const page = Math.max(Number((await searchParams).pagina) || 1, 1);
  const visits = await listMyVisits(session.user.id, page);
  const listings = await Promise.all(visits.items.map((visit) => getListing(visit.listingId).catch(() => null)));

  return (
    <main className="main">
      <div className="container">
        <header className="page-header">
          <p className="eyebrow">Tu cuenta</p>
          <h1>Mis visitas</h1>
          <p>Las visitas que has pedido y en qué van. El propietario tiene un plazo para confirmar uno de tus horarios.</p>
        </header>

        {visits.items.length === 0 ? (
          <div className="surface-card empty-state">
            <h2>Aún no has pedido visitas</h2>
            <p>
              Busca un apartamento que te guste y propón un horario. <Link href="/">Ver publicaciones</Link>
            </p>
          </div>
        ) : (
          <ul className="visit-list">
            {visits.items.map((visit, index) => {
              const listing = listings[index];
              const status = visitStatus(visit.status);

              return (
                <li key={visit.id} className="surface-card visit-item">
                  <div>
                    <span className={`pill pill-${status.tone}`}>{status.label}</span>
                    <h2 style={{ marginTop: "0.5rem" }}>
                      <Link href={`/publicaciones/${visit.listingId}`}>
                        {listing ? place(listing.neighborhood, listing.city) : "Publicación no disponible"}
                      </Link>
                    </h2>
                    <div className="visit-meta">
                      {visit.confirmedSlotStart ? (
                        <span>Confirmada para el {dateTime(visit.confirmedSlotStart)}</span>
                      ) : (
                        <span>Primer horario propuesto: {dateTime(visit.firstSlotStart)}</span>
                      )}
                      {visit.status === "Requested" && <span>Respuesta antes del {dateTime(visit.respondBy)}</span>}
                    </div>
                  </div>
                  {cancellable.includes(visit.status) && <CancelVisitForm visitId={visit.id} />}
                </li>
              );
            })}
          </ul>
        )}

        <Pager page={visits.page} totalPages={visits.totalPages} href={(target) => `/visitas?pagina=${target}`} />
      </div>
    </main>
  );
}

import Link from "next/link";
import { auth } from "@/auth";
import { NotAVisitor } from "@/components/not-a-visitor";
import { Pager } from "@/components/pager";
import { SignInRequired } from "@/components/sign-in-required";
import { listingTitle, shortDate, shortDateTime, slotRange, visitStatus } from "@/lib/format";
import { listMyVisits } from "@/lib/rentals";
import type { Visit } from "@/lib/types";

export const metadata = { title: "Mis visitas" };

const pageSize = 50;

const groups = [
  { title: "Te toca responder", statuses: ["AwaitingVisitor"], active: true },
  { title: "Esperando al propietario", statuses: ["AwaitingHost"], active: true },
  { title: "Agendadas", statuses: ["Scheduled"], active: true },
  { title: "Historial", statuses: ["Completed", "NoShow", "Cancelled", "Expired"], active: false },
];

const time = (value: string | null) => (value ? new Date(value).getTime() : Number.MAX_SAFE_INTEGER);

function nextStep(visits: Visit[]) {
  const earliest = (status: string, at: (visit: Visit) => string | null) =>
    visits
      .filter((visit) => visit.status === status)
      .sort((a, b) => time(at(a)) - time(at(b)))[0];

  return (
    earliest("AwaitingVisitor", (visit) => visit.respondBy) ??
    earliest("Scheduled", (visit) => visit.scheduledStartsAt) ??
    visits.find((visit) => visit.status === "AwaitingHost")
  );
}

function when(visit: Visit) {
  switch (visit.status) {
    case "AwaitingVisitor":
    case "AwaitingHost":
      return { label: "Responde antes del", value: visit.respondBy ? shortDateTime(visit.respondBy) : "—" };
    case "Scheduled":
      return {
        label: "Visita",
        value: visit.scheduledStartsAt ? `${shortDate(visit.scheduledStartsAt)}, ${slotRange(visit.scheduledStartsAt)}` : "—",
      };
    case "Completed":
    case "NoShow":
      return { label: "Fue el", value: visit.scheduledStartsAt ? shortDateTime(visit.scheduledStartsAt) : "—" };
    default:
      return { label: "Actualizada el", value: shortDate(visit.updatedAt) };
  }
}

function NextStep({ visit }: { visit: Visit }) {
  const listing = listingTitle(visit.listingType, visit.listingNeighborhood);
  const host = visit.hostName ?? "El propietario";

  const content =
    visit.status === "AwaitingVisitor"
      ? {
          eyebrow: "Lo que sigue · te toca responder",
          title: `${host} te propuso otros horarios para ${listing.toLowerCase()}.`,
          body: visit.respondBy ? (
            <>
              Acepta uno o propón otros antes del <strong>{shortDateTime(visit.respondBy)}</strong>. Si no respondes,
              la visita vence.
            </>
          ) : (
            <>Acepta uno o propón otros.</>
          ),
          action: "Responder ahora",
        }
      : visit.status === "Scheduled"
        ? {
            eyebrow: "Lo que sigue · visita agendada",
            title: `${listing} con ${host}.`,
            body: visit.scheduledStartsAt ? (
              <>
                Te esperan el{" "}
                <strong>{`${shortDate(visit.scheduledStartsAt)}, ${slotRange(visit.scheduledStartsAt)}`}</strong>.
              </>
            ) : null,
            action: "Ver la visita",
          }
        : {
            eyebrow: "Lo que sigue · esperando al propietario",
            title: `${host} aún no responde tu solicitud para ${listing.toLowerCase()}.`,
            body: <>Te avisaremos cuando acepte uno de tus horarios o te proponga otros.</>,
            action: "Ver la visita",
          };

  return (
    <section
      className="card card-lime"
      aria-label="Lo que sigue"
      style={{ flexDirection: "row", flexWrap: "wrap", alignItems: "center", gap: "16px 24px" }}
    >
      <div className="stack" style={{ flex: "1 1 420px", "--gap": "6px" } as React.CSSProperties}>
        <p className="eyebrow">{content.eyebrow}</p>
        <h2 className="h2" style={{ fontSize: 24, letterSpacing: "-0.02em" }}>
          {content.title}
        </h2>
        {content.body && <p>{content.body}</p>}
      </div>
      <Link className="btn btn-ink" href={`/visitas/${visit.id}`}>
        {content.action}
      </Link>
    </section>
  );
}

export default async function VisitsPage({ searchParams }: { searchParams: Promise<{ pagina?: string }> }) {
  const session = await auth();
  if (!session) return <SignInRequired message="Ingresa para ver las visitas que has pedido." />;
  if (session.user.role !== "Visitor") return <NotAVisitor />;

  const page = Math.max(Number((await searchParams).pagina) || 1, 1);
  const [visits, noShows] = await Promise.all([
    listMyVisits({ page, pageSize }),
    listMyVisits({ status: "NoShow", pageSize: 1 }),
  ]);
  const next = nextStep(visits.items);

  return (
    <main className="main" style={{ paddingTop: 36 }}>
      <div className="container container-narrow stack" style={{ "--gap": "24px" } as React.CSSProperties}>
        <div className="row row-between row-end" style={{ "--gap": "16px" } as React.CSSProperties}>
          <div className="stack" style={{ "--gap": "6px" } as React.CSSProperties}>
            <p className="eyebrow">Tu agenda</p>
            <h1 className="title">Mis visitas</h1>
          </div>
          <div className="row">
            <div className="stat">
              <p className="eyebrow">Solicitadas</p>
              <p className="stat-value">{visits.totalCount}</p>
            </div>
            <div className="stat">
              <p className="eyebrow">Inasistencias</p>
              <p className="stat-value">{noShows.totalCount}</p>
            </div>
          </div>
        </div>

        {visits.items.length === 0 ? (
          <div className="empty">
            <p className="h2">Aún no has pedido visitas</p>
            <p>
              Cuando encuentres un inmueble que te guste, propón hasta tres horarios y aquí verás la respuesta del
              propietario.
            </p>
            <Link className="btn btn-ink" href="/">
              Buscar inmuebles
            </Link>
          </div>
        ) : (
          <>
            {next && <NextStep visit={next} />}

            {groups.map((group) => {
              const items = visits.items.filter((visit) => group.statuses.includes(visit.status));
              if (items.length === 0) return null;

              return (
                <section key={group.title} className="stack" style={{ "--gap": "10px" } as React.CSSProperties}>
                  <h2 className="h2 h2-sm row" style={{ alignItems: "baseline" }}>
                    {group.title} <span className="count">{items.length}</span>
                  </h2>
                  {items.map((visit) => {
                    const status = visitStatus(visit.status);
                    const moment = when(visit);

                    return (
                      <Link key={visit.id} className="visit-row" href={`/visitas/${visit.id}`}>
                        <span className={group.active ? "tile tile-active" : "tile"}>
                          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                            <path d="M4 20V9l8-5 8 5v11M9 20v-6h6v6" />
                          </svg>
                        </span>
                        <div className="visit-row-main">
                          <p style={{ fontWeight: 600 }}>{listingTitle(visit.listingType, visit.listingNeighborhood)}</p>
                          <p className="small muted">{[visit.listingCity, visit.hostName].filter(Boolean).join(" · ")}</p>
                        </div>
                        <div className="visit-row-when">
                          <p className="eyebrow" style={{ fontSize: 11 }}>
                            {moment.label}
                          </p>
                          <p style={{ fontWeight: 500 }}>{moment.value}</p>
                        </div>
                        <span className={`pill pill-${status.tone}`} style={{ alignSelf: "center" }}>
                          {status.label}
                        </span>
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="muted">
                          <path d="M9 6l6 6-6 6" />
                        </svg>
                      </Link>
                    );
                  })}
                </section>
              );
            })}

            <Pager page={visits.page} totalPages={visits.totalPages} href={(target) => `/visitas?pagina=${target}`} />
          </>
        )}
      </div>
    </main>
  );
}

import Link from "next/link";
import { notFound } from "next/navigation";
import { auth } from "@/auth";
import { SignInRequired } from "@/components/sign-in-required";
import {
  dayParts,
  historyText,
  listingTitle,
  shortDate,
  shortDateTime,
  slotRange,
  visitRules,
  visitStatus,
} from "@/lib/format";
import { ApiError, getVisit } from "@/lib/rentals";
import type { VisitHistoryEntry } from "@/lib/types";
import { VisitActions } from "./visit-actions";

export const metadata = { title: "Visita" };

async function load(id: string) {
  try {
    return await getVisit(id);
  } catch (error) {
    if (error instanceof ApiError && (error.status === 404 || error.status === 403)) notFound();
    throw error;
  }
}

const dotClass = (entry: VisitHistoryEntry) =>
  entry.action === "Scheduled" || entry.action === "Completed"
    ? "timeline-dot timeline-dot-done"
    : entry.action === "NoShow"
      ? "timeline-dot timeline-dot-bad"
      : "timeline-dot";

export default async function VisitPage({ params }: { params: Promise<{ id: string }> }) {
  const session = await auth();
  if (!session) return <SignInRequired message="Ingresa para ver y responder tus visitas." />;

  const visit = await load((await params).id);
  const now = Date.now();
  const amHost = visit.hostId === session.user.id;
  const counterpart = (amHost ? visit.visitorName : visit.hostName) ?? (amHost ? "El visitante" : "El propietario");
  const status = visitStatus(visit.status);
  const negotiating = visit.status === "AwaitingVisitor" || visit.status === "AwaitingHost";
  const myTurn = (visit.status === "AwaitingVisitor" && !amHost) || (visit.status === "AwaitingHost" && amHost);
  const startsAt = visit.scheduledStartsAt ? new Date(visit.scheduledStartsAt).getTime() : null;
  const beforeStart = startsAt === null || now < startsAt;
  const canCancel = (negotiating || visit.status === "Scheduled") && beforeStart;
  const lateCancellation =
    !amHost && startsAt !== null && startsAt - now < visitRules.lateCancellationHours * 60 * 60 * 1000;
  const cancelledByMe = visit.cancelledBy === (amHost ? "Host" : "Visitor");
  const history = [...visit.history].reverse();
  const scheduled = visit.scheduledStartsAt ? dayParts(visit.scheduledStartsAt) : null;
  const title = listingTitle(visit.listingType, visit.listingNeighborhood);

  return (
    <main className="main" style={{ paddingTop: 24 }}>
      <div className="container container-narrow stack" style={{ "--gap": "20px" } as React.CSSProperties}>
        <Link className="back-link" href="/visitas">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M15 18l-6-6 6-6" />
          </svg>
          Mis visitas
        </Link>

        <div className="stack" style={{ "--gap": "8px" } as React.CSSProperties}>
          <span className={`pill pill-lg pill-${status.tone}`}>{status.label}</span>
          <h1 className="title">{title}</h1>
          <p className="muted">
            {[visit.listingCity, `con ${counterpart}`].filter(Boolean).join(" · ")} ·{" "}
            <Link href={`/publicaciones/${visit.listingId}`}>Ver publicación</Link>
          </p>
        </div>

        <div className="split">
          <div className="split-main">
            {myTurn && visit.respondBy && (
              <section className="card card-lime row" style={{ padding: "18px 22px", "--gap": "8px 16px" } as React.CSSProperties}>
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <circle cx="12" cy="12" r="9" />
                  <path d="M12 7v5l3 2" />
                </svg>
                <p style={{ flex: "1 1 300px" }}>
                  Tienes hasta el <strong>{shortDateTime(visit.respondBy)}</strong> para responder. Después la visita
                  vence.
                </p>
              </section>
            )}

            {negotiating && !myTurn && (
              <section className="card" style={{ "--gap": "14px" } as React.CSSProperties}>
                <div className="stack" style={{ "--gap": "4px" } as React.CSSProperties}>
                  <h2 className="h2">Le toca responder a {counterpart}</h2>
                  <p className="muted">
                    {visit.respondBy ? (
                      <>
                        Tiene hasta el <strong style={{ color: "var(--ink)" }}>{shortDateTime(visit.respondBy)}</strong>{" "}
                        para aceptar uno de tus horarios o proponerte otros.
                      </>
                    ) : (
                      "Te avisaremos cuando responda."
                    )}
                  </p>
                </div>
                <div className="stack" style={{ "--gap": "6px" } as React.CSSProperties}>
                  {visit.proposedSlots.map((slot) => (
                    <div className="slot-static" key={slot}>
                      <span style={{ fontWeight: 600 }}>{shortDate(slot)}</span>
                      <span className="mono small muted">{slotRange(slot)}</span>
                    </div>
                  ))}
                </div>
              </section>
            )}

            {visit.status === "Scheduled" && scheduled && visit.scheduledStartsAt && (
              <>
                <section className="card card-dark row" style={{ padding: 24, "--gap": "20px" } as React.CSSProperties}>
                  <div className="date-tile">
                    {scheduled.weekday}
                    <strong>{scheduled.day}</strong>
                    {scheduled.month}
                  </div>
                  <div className="stack" style={{ flex: "1 1 280px", "--gap": "4px" } as React.CSSProperties}>
                    <p className="eyebrow">Visita agendada</p>
                    <p style={{ font: "700 28px/1.1 var(--font-display)" }}>{slotRange(visit.scheduledStartsAt)}</p>
                    <p className="muted">{[title, visit.listingCity].filter(Boolean).join(", ")}</p>
                  </div>
                </section>
                {canCancel && !amHost && (
                  <p className="note">
                    Si necesitas cancelar, hazlo con tiempo: a menos de 12 horas de la visita queda registrada como
                    cancelación tardía.
                  </p>
                )}
              </>
            )}

            {visit.status === "Completed" && (
              <section className="card card-info" style={{ "--gap": "4px" } as React.CSSProperties}>
                <h2 className="h2">Visita realizada</h2>
                {visit.scheduledStartsAt && (
                  <p>
                    {visit.closedBy === "System" ? "Se cerró automáticamente" : `${visit.hostName ?? "El propietario"} confirmó`} la
                    visita del {shortDate(visit.scheduledStartsAt)}, {slotRange(visit.scheduledStartsAt)}.
                  </p>
                )}
              </section>
            )}

            {visit.status === "NoShow" && (
              <section className="card card-danger" style={{ "--gap": "4px" } as React.CSSProperties}>
                <h2 className="h2">Registrada como inasistencia</h2>
                <p>
                  {visit.hostName ?? "El propietario"} indicó que no llegaste a la visita
                  {visit.scheduledStartsAt ? ` del ${shortDateTime(visit.scheduledStartsAt)}` : ""}. Queda en tu
                  historial de visitas.
                </p>
              </section>
            )}

            {visit.status === "Cancelled" && (
              <section className="card" style={{ "--gap": "8px" } as React.CSSProperties}>
                <div className="row">
                  <h2 className="h2">{cancelledByMe ? "Cancelaste esta visita" : `${counterpart} canceló la visita`}</h2>
                  {visit.isLateCancellation && <span className="pill pill-warning">Cancelación tardía</span>}
                </div>
                {visit.cancellationReason && (
                  <>
                    <p className="muted">Motivo</p>
                    <p className="slot-static">{visit.cancellationReason}</p>
                  </>
                )}
              </section>
            )}

            {visit.status === "Expired" && (
              <section className="card card-dashed" style={{ "--gap": "8px" } as React.CSSProperties}>
                <h2 className="h2">La visita venció</h2>
                <p className="muted">
                  Nadie respondió
                  {visit.respondBy ? ` antes del ${shortDateTime(visit.respondBy)}` : " a tiempo"}. Si el inmueble sigue
                  publicado, puedes pedir una visita nueva.
                </p>
                <Link className="btn btn-ink btn-start" href={`/publicaciones/${visit.listingId}`}>
                  Pedir otra visita
                </Link>
              </section>
            )}

            <VisitActions
              key={visit.updatedAt}
              visitId={visit.id}
              now={now}
              counterpart={counterpart}
              canRespond={myTurn}
              canCancel={canCancel}
              proposedSlots={visit.proposedSlots}
              lateCancellation={lateCancellation}
            />
          </div>

          <aside className="split-aside card">
            <h2 className="h2 h2-sm">Historial</h2>
            <ol className="timeline">
              {history.map((entry, index) => (
                <li key={`${entry.at}-${index}`}>
                  <div className="timeline-rail">
                    <span className={dotClass(entry)} />
                    <span className="timeline-line" />
                  </div>
                  <div className="timeline-body">
                    <p className="mono small muted" style={{ fontSize: 12 }}>
                      {shortDateTime(entry.at)}
                    </p>
                    <p style={{ fontWeight: 600 }}>
                      {historyText(entry, visit.hostName ?? "El propietario")}
                      {entry.isLate && <span className="pill pill-warning" style={{ marginLeft: 8 }}>Tardía</span>}
                    </p>
                    {entry.slots.length > 0 && (
                      <div className="row" style={{ "--gap": "4px" } as React.CSSProperties}>
                        {entry.slots.map((slot) => (
                          <span className="slot-chip" key={slot}>
                            {shortDateTime(slot)}
                          </span>
                        ))}
                      </div>
                    )}
                    {entry.reason && <p className="small muted">“{entry.reason}”</p>}
                  </div>
                </li>
              ))}
            </ol>
          </aside>
        </div>
      </div>
    </main>
  );
}

import { auth } from "@/auth";
import { logout } from "@/app/actions";
import { NotAVisitor } from "@/components/not-a-visitor";
import { SignInRequired } from "@/components/sign-in-required";
import { activityText, longDate, shortDateTime } from "@/lib/format";
import { getMyVisitorDetail } from "@/lib/rentals";
import type { VisitorActivity } from "@/lib/types";

export const metadata = { title: "Mi cuenta" };

const activityDetail = (activity: VisitorActivity) =>
  [
    activity.listingNeighborhood && `Visita en ${activity.listingNeighborhood}`,
    activity.hostName && !activityText(activity).startsWith(activity.hostName) && activity.hostName,
    activity.slot && shortDateTime(activity.slot),
  ]
    .filter(Boolean)
    .join(" · ");

export default async function AccountPage() {
  const session = await auth();
  if (!session) return <SignInRequired message="Ingresa para ver tus datos y tu actividad." />;

  if (session.user.role !== "Visitor") return <NotAVisitor />;

  const { visitor, activity } = await getMyVisitorDetail(session.user.id);

  return (
    <main className="main" style={{ paddingTop: 36 }}>
      <div className="container container-narrow stack" style={{ "--gap": "24px" } as React.CSSProperties}>
        <div className="stack" style={{ "--gap": "6px" } as React.CSSProperties}>
          <p className="eyebrow">Visitante desde el {longDate(visitor.registeredAt)}</p>
          <h1 className="title">{visitor.name}</h1>
        </div>

        <div className="split">
          <div className="split-aside">
            <section className="card">
              <h2 className="h2 h2-sm">Tus datos</h2>
              <dl className="stack" style={{ "--gap": "14px" } as React.CSSProperties}>
                <div className="kv">
                  <dt>Nombre</dt>
                  <dd>{visitor.name}</dd>
                </div>
                <div className="kv">
                  <dt>Correo</dt>
                  <dd>
                    {visitor.email ? (
                      <>
                        {visitor.email} <span className="pill pill-success">Confirmado</span>
                      </>
                    ) : (
                      <span className="muted">Sin confirmar</span>
                    )}
                  </dd>
                </div>
                <div className="kv">
                  <dt>Celular</dt>
                  <dd>
                    {visitor.phone ? (
                      <>
                        <span className="mono">{visitor.phone}</span> <span className="pill pill-success">Confirmado</span>
                      </>
                    ) : (
                      <span className="muted">Sin confirmar</span>
                    )}
                  </dd>
                </div>
              </dl>
              <p className="note">
                Estos datos vienen de tu cuenta Llave y son los que ve el propietario cuando pides una visita.
              </p>
            </section>

            <div className="row" style={{ display: "grid", gridTemplateColumns: "repeat(2, minmax(0, 1fr))", gap: 12 }}>
              <div className="stat stat-dark">
                <p className="eyebrow">Visitas pedidas</p>
                <p className="stat-value" style={{ fontSize: 36 }}>
                  {visitor.visitCount}
                </p>
              </div>
              <div className="stat">
                <p className="eyebrow">Inasistencias</p>
                <p className="stat-value" style={{ fontSize: 36 }}>
                  {visitor.noShowCount}
                </p>
              </div>
            </div>

            <form action={logout}>
              <button className="btn">Cerrar sesión</button>
            </form>
          </div>

          <section className="split-main card">
            <div className="stack" style={{ "--gap": "4px" } as React.CSSProperties}>
              <h2 className="h2 h2-sm">Actividad</h2>
              <p className="small muted">Todo lo que ha pasado con tus visitas y tu cuenta, lo más reciente primero.</p>
            </div>
            <ol className="activity">
              {activity.map((entry, index) => {
                const extra = activityDetail(entry);
                return (
                  <li key={`${entry.at}-${index}`}>
                    <span className="activity-at">{shortDateTime(entry.at)}</span>
                    <div className="activity-body">
                      <p className="row">
                        <span style={{ fontWeight: 600 }}>{activityText(entry)}</span>
                        {entry.isLate && <span className="pill pill-warning">Tardía</span>}
                      </p>
                      {extra && <p className="small muted">{extra}</p>}
                    </div>
                  </li>
                );
              })}
            </ol>
          </section>
        </div>
      </div>
    </main>
  );
}

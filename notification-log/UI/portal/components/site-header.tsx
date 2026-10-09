import Link from "next/link";
import { auth } from "@/auth";
import { LoginButton } from "@/components/login-button";
import { LlaveMark } from "@/components/llave-mark";
import { NavLink } from "@/components/nav-link";
import { initials } from "@/lib/format";
import { listMyVisits } from "@/lib/rentals";

async function pendingVisits() {
  try {
    return (await listMyVisits({ status: "AwaitingVisitor", pageSize: 1 })).totalCount;
  } catch {
    return 0;
  }
}

export async function SiteHeader() {
  const session = await auth();
  const pending = session?.user.role === "Visitor" ? await pendingVisits() : 0;
  const name = session?.user.name ?? "";

  return (
    <header className="site-header">
      <div className="container">
        <Link className="brand" href="/">
          <span className="brand-mark">
            <LlaveMark />
          </span>
          <span>
            <span className="brand-name">Llave</span>
            <span className="brand-tag">Arriendos y ventas</span>
          </span>
        </Link>

        <nav className="site-nav" aria-label="Principal">
          <NavLink href="/" match={["/", "/publicaciones"]}>
            Buscar
          </NavLink>
          {session ? (
            <>
              <NavLink href="/visitas" match={["/visitas"]}>
                Mis visitas
                {pending > 0 && (
                  <span className="nav-badge" aria-label={`${pending} por responder`}>
                    {pending}
                  </span>
                )}
              </NavLink>
              <NavLink href="/cuenta" match={["/cuenta"]}>
                <span className="avatar" aria-hidden="true">
                  {initials(name) || "?"}
                </span>
                {name.split(" ")[0] || "Mi cuenta"}
              </NavLink>
            </>
          ) : (
            <LoginButton className="btn btn-lime">Ingresar o crear cuenta</LoginButton>
          )}
        </nav>
      </div>
    </header>
  );
}

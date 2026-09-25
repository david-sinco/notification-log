import Link from "next/link";
import { auth } from "@/auth";
import { LoginButton } from "@/components/login-button";
import { logout } from "@/app/actions";

export async function SiteHeader() {
  const session = await auth();

  return (
    <header className="site-header">
      <div className="container">
        <Link className="brand" href="/">
          <img src="/llave-mark.svg" alt="" />
          <span>
            <span className="brand-name">Llave</span>
            <span className="brand-tag">Arriendos y ventas</span>
          </span>
        </Link>

        <nav className="site-nav">
          <Link href="/">Buscar</Link>
          {session ? (
            <>
              <Link href="/visitas">Mis visitas</Link>
              <Link href="/perfil">Mi perfil</Link>
              <span className="user-chip">{session.user.name}</span>
              <form action={logout}>
                <button className="btn btn-secondary btn-sm">Cerrar sesión</button>
              </form>
            </>
          ) : (
            <LoginButton className="btn btn-primary btn-sm">Iniciar sesión</LoginButton>
          )}
        </nav>
      </div>
    </header>
  );
}

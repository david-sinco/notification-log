import Link from "next/link";

export function NotAVisitor() {
  return (
    <main className="main">
      <div className="container container-narrow">
        <div className="empty">
          <h1 className="h2">Tu cuenta no es de visitante</h1>
          <p>
            Este portal es para quienes buscan dónde vivir. Si eres propietario, gestiona tus publicaciones y visitas
            desde el backoffice de Llave.
          </p>
          <Link className="btn btn-ink" href="/">
            Volver a la búsqueda
          </Link>
        </div>
      </div>
    </main>
  );
}

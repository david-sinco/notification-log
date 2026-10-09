import Link from "next/link";

export default function NotFound() {
  return (
    <main className="main">
      <div className="container container-narrow">
        <div className="empty">
          <h1 className="h2">No encontramos esta página</h1>
          <p>Puede que la publicación ya no esté disponible o que el enlace esté incompleto.</p>
          <Link className="btn btn-ink" href="/">
            Volver a la búsqueda
          </Link>
        </div>
      </div>
    </main>
  );
}

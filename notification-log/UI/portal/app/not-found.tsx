import Link from "next/link";

export default function NotFound() {
  return (
    <main className="main">
      <div className="container">
        <div className="surface-card empty-state">
          <h2>No encontramos esta página</h2>
          <p>
            Puede que la publicación ya no esté disponible. <Link href="/">Volver a la búsqueda</Link>
          </p>
        </div>
      </div>
    </main>
  );
}

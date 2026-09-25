import Link from "next/link";

export function Pager({ page, totalPages, href }: { page: number; totalPages: number; href: (page: number) => string }) {
  if (totalPages <= 1) return null;

  return (
    <nav className="pager" aria-label="Paginación">
      {page > 1 && (
        <Link className="btn btn-secondary btn-sm" href={href(page - 1)}>
          Anterior
        </Link>
      )}
      <span>
        Página {page} de {totalPages}
      </span>
      {page < totalPages && (
        <Link className="btn btn-secondary btn-sm" href={href(page + 1)}>
          Siguiente
        </Link>
      )}
    </nav>
  );
}

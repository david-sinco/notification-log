import Link from "next/link";

export function Pager({ page, totalPages, href }: { page: number; totalPages: number; href: (page: number) => string }) {
  if (totalPages <= 1) return null;

  const first = Math.max(1, Math.min(page - 2, totalPages - 4));
  const pages = Array.from({ length: Math.min(5, totalPages) }, (_, index) => first + index);

  return (
    <nav className="pager" aria-label="Paginación">
      {page > 1 && (
        <Link className="pager-link" href={href(page - 1)} aria-label="Página anterior">
          ‹
        </Link>
      )}
      {pages.map((target) => (
        <Link
          key={target}
          className="pager-link"
          href={href(target)}
          aria-current={target === page ? "page" : undefined}
        >
          {target}
        </Link>
      ))}
      {page < totalPages && (
        <Link className="pager-link" href={href(page + 1)} aria-label="Página siguiente">
          ›
        </Link>
      )}
    </nav>
  );
}

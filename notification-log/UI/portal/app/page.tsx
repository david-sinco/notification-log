import Link from "next/link";
import { ListingCard } from "@/components/listing-card";
import { Pager } from "@/components/pager";
import { listCatalog } from "@/lib/rentals";

type SearchParams = Promise<{ q?: string; operacion?: string; pagina?: string }>;

const filters = [
  { value: "", operation: undefined, label: "Arriendo y venta", heading: "Publicaciones disponibles" },
  { value: "arriendo", operation: "Rent", label: "Arriendo", heading: "En arriendo" },
  { value: "venta", operation: "Sale", label: "Venta", heading: "En venta" },
];

export default async function CatalogPage({ searchParams }: { searchParams: SearchParams }) {
  const { q = "", operacion = "", pagina } = await searchParams;
  const filter = filters.find((candidate) => candidate.value === operacion) ?? filters[0];
  const page = Math.max(Number(pagina) || 1, 1);
  const result = await listCatalog(q, filter.operation, page);

  const href = (values: { operacion?: string; pagina?: number }) => {
    const params = new URLSearchParams();
    if (q) params.set("q", q);
    const operation = values.operacion ?? filter.value;
    if (operation) params.set("operacion", operation);
    if (values.pagina && values.pagina > 1) params.set("pagina", String(values.pagina));
    const query = params.toString();
    return query ? `/?${query}` : "/";
  };

  return (
    <>
      <section className="hero">
        <div className="container">
          <p className="hero-eyebrow">Publicaciones revisadas por nuestro equipo</p>
          <h1 className="hero-title">Encuentra dónde vas a vivir y agenda la visita con su dueño.</h1>
          <p className="hero-lead">
            Apartamentos y apartaestudios en arriendo y venta. Propones hasta tres horarios, el propietario acepta uno
            o te ofrece otros.
          </p>

          <form className="search-bar" action="/" role="search">
            <label className="sr-only" htmlFor="q">
              Ciudad, barrio o dirección
            </label>
            <input id="q" className="input" name="q" defaultValue={q} placeholder="Ciudad, barrio o dirección" />
            {filter.value && <input type="hidden" name="operacion" value={filter.value} />}
            <button className="btn btn-lime">Buscar</button>
          </form>

          <nav className="chips" aria-label="Operación">
            {filters.map((candidate) => (
              <Link
                key={candidate.value}
                className="chip"
                href={href({ operacion: candidate.value })}
                aria-current={candidate === filter ? "true" : undefined}
              >
                {candidate.label}
              </Link>
            ))}
          </nav>
        </div>
      </section>

      <main className="main">
        <div className="container stack" style={{ "--gap": "20px" } as React.CSSProperties}>
          <div className="row row-between" style={{ alignItems: "baseline" }}>
            <h2 className="h2">{q ? `${filter.heading} para “${q}”` : filter.heading}</h2>
            <p className="eyebrow">
              {result.totalCount === 1 ? "1 resultado" : `${result.totalCount} resultados`} · más recientes primero
            </p>
          </div>

          {result.items.length === 0 ? (
            <div className="empty">
              <p className="h2">No encontramos publicaciones</p>
              <p>Prueba con otra ciudad o barrio, o quita el filtro de operación.</p>
            </div>
          ) : (
            <div className="listing-grid">
              {result.items.map((listing) => (
                <ListingCard key={listing.id} listing={listing} />
              ))}
            </div>
          )}

          <Pager page={result.page} totalPages={result.totalPages} href={(target) => href({ pagina: target })} />
        </div>
      </main>
    </>
  );
}

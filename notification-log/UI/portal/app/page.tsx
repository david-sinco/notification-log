import { ListingCard } from "@/components/listing-card";
import { Pager } from "@/components/pager";
import { listCatalog } from "@/lib/rentals";

type SearchParams = Promise<{ q?: string; operacion?: string; pagina?: string }>;

const operations: Record<string, string> = { arriendo: "Rent", venta: "Sale" };

export default async function CatalogPage({ searchParams }: { searchParams: SearchParams }) {
  const { q = "", operacion = "", pagina } = await searchParams;
  const page = Math.max(Number(pagina) || 1, 1);
  const result = await listCatalog(q, operations[operacion], page);

  const href = (target: number) => `/?${new URLSearchParams({ q, operacion, pagina: String(target) })}`;

  return (
    <>
      <section className="hero">
        <div className="container">
          <p className="eyebrow">Portal inmobiliario</p>
          <h1>Encuentra el lugar que vas a llamar casa.</h1>
          <p>Apartamentos en arriendo y venta, publicados por sus propietarios y revisados por nuestro equipo.</p>

          <form className="search-bar" action="/">
            <input className="input" name="q" defaultValue={q} placeholder="Ciudad, barrio o dirección" />
            <select className="input" name="operacion" defaultValue={operacion}>
              <option value="">Arriendo y venta</option>
              <option value="arriendo">Arriendo</option>
              <option value="venta">Venta</option>
            </select>
            <button className="btn btn-primary">Buscar</button>
          </form>
        </div>
      </section>

      <main className="main">
        <div className="container">
          <div className="results-meta">
            <h2 className="section-title">Publicaciones disponibles</h2>
            <span>{result.totalCount === 1 ? "1 resultado" : `${result.totalCount} resultados`}</span>
          </div>

          {result.items.length === 0 ? (
            <div className="surface-card empty-state">
              <h2>No encontramos publicaciones</h2>
              <p>Prueba con otra ciudad o barrio, o quita el filtro de operación.</p>
            </div>
          ) : (
            <div className="listing-grid">
              {result.items.map((listing) => (
                <ListingCard key={listing.id} listing={listing} />
              ))}
            </div>
          )}

          <Pager page={result.page} totalPages={result.totalPages} href={href} />
        </div>
      </main>
    </>
  );
}

import Link from "next/link";
import { LlaveMark } from "@/components/llave-mark";
import { listingTitle, money, operationLabel } from "@/lib/format";
import type { ListingSummary } from "@/lib/types";

export function ListingCard({ listing }: { listing: ListingSummary }) {
  const rent = listing.operation === "Rent";

  return (
    <Link className="listing-card" href={`/publicaciones/${listing.id}`}>
      <div className={listing.coverUrl ? "listing-cover" : "listing-cover listing-cover-empty"}>
        {listing.coverUrl ? <img src={listing.coverUrl} alt="" /> : <LlaveMark size={40} />}
        <span className={`pill cover-tag ${rent ? "pill-lime" : "pill-ink"}`}>{operationLabel(listing.operation)}</span>
        {listing.photoCount > 0 && (
          <span className="photo-count">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <rect x="3" y="5" width="18" height="14" rx="2" />
              <circle cx="9" cy="11" r="2" />
              <path d="M21 17l-5-5-9 7" />
            </svg>
            {listing.photoCount === 1 ? "1 foto" : `${listing.photoCount} fotos`}
          </span>
        )}
      </div>
      <div className="listing-body">
        <p className="listing-price">
          {money(listing.price)}
          {rent && listing.price !== null && <span className="listing-per"> /mes</span>}
        </p>
        <p style={{ fontWeight: 600 }}>{listingTitle(listing.type, listing.neighborhood)}</p>
        <p className="small muted">{listing.city ?? "Ciudad por confirmar"}</p>
        <div className="listing-specs">
          {listing.bedrooms !== null && <span>{listing.bedrooms} hab.</span>}
          {listing.area !== null && <span>{listing.area} m²</span>}
          {listing.ownerName && <span>{listing.ownerName}</span>}
        </div>
      </div>
    </Link>
  );
}

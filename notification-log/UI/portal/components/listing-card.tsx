import Link from "next/link";
import { money, operationLabel, place, propertyTypeLabel } from "@/lib/format";
import type { ListingSummary } from "@/lib/types";

export function ListingCard({ listing }: { listing: ListingSummary }) {
  return (
    <Link className="listing-card" href={`/publicaciones/${listing.id}`}>
      <div className="listing-cover">
        <span className="pill pill-brand">{operationLabel(listing.operation)}</span>
        <img src="/llave-mark.svg" alt="" />
      </div>
      <div className="listing-body">
        <span className="listing-price">{money(listing.price)}</span>
        <span className="listing-place">{place(listing.neighborhood, listing.city)}</span>
        <span className="form-hint">{propertyTypeLabel(listing.type)}</span>
        <div className="listing-specs">
          {listing.bedrooms !== null && <span>{listing.bedrooms} hab.</span>}
          {listing.area !== null && <span>{listing.area} m²</span>}
        </div>
      </div>
    </Link>
  );
}

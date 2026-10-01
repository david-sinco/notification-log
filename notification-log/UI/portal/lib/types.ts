export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type ListingSummary = {
  id: string;
  operation: string;
  status: string;
  type: string | null;
  city: string | null;
  neighborhood: string | null;
  price: number | null;
  bedrooms: number | null;
  area: number | null;
};

export type Listing = ListingSummary & {
  bathrooms: number | null;
  parkingSpots: number | null;
  stratum: number | null;
  floor: number | null;
  hasElevator: boolean;
  administrationFee: number | null;
  address: string | null;
  description: string | null;
  photos: string[];
  expiresAt: string | null;
};

export type Visit = {
  id: string;
  listingId: string;
  hostId: string;
  visitorId: string;
  status: string;
  proposedSlots: string[];
  respondBy: string | null;
  scheduledStartsAt: string | null;
  scheduledEndsAt: string | null;
  cancelledBy: string | null;
  cancellationReason: string | null;
  isLateCancellation: boolean;
  closedBy: string | null;
};

export type Visitor = {
  id: string;
  status: string;
  displayName: string;
  firstNames: string | null;
  lastNames: string | null;
  documentType: string | null;
  documentNumber: string | null;
  email: string;
  phone: string;
};

export type ActionResult = { error?: string };

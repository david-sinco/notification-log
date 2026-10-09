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
  ownerName: string | null;
  coverUrl: string | null;
  photoCount: number;
};

export type Listing = {
  id: string;
  ownerUserId: string | null;
  operation: string;
  status: string;
  type: string | null;
  area: number | null;
  bedrooms: number | null;
  bathrooms: number | null;
  parkingSpots: number | null;
  stratum: number | null;
  floor: number | null;
  hasElevator: boolean;
  administrationFee: number | null;
  city: string | null;
  neighborhood: string | null;
  address: string | null;
  description: string | null;
  price: number | null;
  photos: { fileName: string; url: string }[];
  expiresAt: string | null;
  ownerName: string | null;
};

export type VisitParty = "Visitor" | "Host" | "System";

export type VisitHistoryEntry = {
  at: string;
  by: VisitParty;
  action: string;
  slots: string[];
  reason: string | null;
  isLate: boolean;
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
  cancelledBy: VisitParty | null;
  cancellationReason: string | null;
  isLateCancellation: boolean;
  closedBy: VisitParty | null;
  requestedAt: string;
  updatedAt: string;
  listingType: string | null;
  listingNeighborhood: string | null;
  listingCity: string | null;
  visitorName: string | null;
  hostName: string | null;
  history: VisitHistoryEntry[];
};

export type VisitorNextStep = {
  visitId: string;
  status: string;
  at: string;
  listingNeighborhood: string | null;
};

export type Visitor = {
  id: string;
  name: string;
  email: string;
  phone: string;
  registeredAt: string;
  visitCount: number;
  noShowCount: number;
  nextStep: VisitorNextStep | null;
};

export type VisitorVisit = {
  visitId: string;
  listingId: string;
  listingType: string | null;
  listingNeighborhood: string | null;
  listingCity: string | null;
  hostName: string;
  status: string;
  firstSlot: string | null;
  respondBy: string | null;
  requestedAt: string;
  updatedAt: string;
};

export type VisitorActivity = {
  at: string;
  action: string;
  by: VisitParty | null;
  visitId: string | null;
  listingNeighborhood: string | null;
  hostName: string | null;
  slot: string | null;
  isLate: boolean;
};

export type VisitorDetail = {
  visitor: Visitor;
  visits: VisitorVisit[];
  activity: VisitorActivity[];
};

export type ActionResult = { error?: string };

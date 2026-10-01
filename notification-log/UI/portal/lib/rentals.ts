import "server-only";
import { headers } from "next/headers";
import { getToken } from "next-auth/jwt";
import type { Listing, ListingSummary, PagedResult, Visit, Visitor } from "@/lib/types";

const baseUrl =
  process.env.services__rental__https__0 ?? process.env.services__rental__http__0 ?? "https://localhost:7057";

export class ApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message);
  }
}

async function accessToken() {
  const token = await getToken({
    req: { headers: await headers() },
    secret: process.env.AUTH_SECRET,
    secureCookie: process.env.AUTH_URL?.startsWith("https://") ?? false,
  });

  if (!token?.accessToken || !token.expiresAt || Date.now() >= token.expiresAt * 1000)
    throw new ApiError("Tu sesión terminó. Inicia sesión de nuevo.", 401);

  return token.accessToken;
}

async function toApiError(response: Response) {
  try {
    const problem = await response.json();
    const errors = problem.errors ? Object.values<string[]>(problem.errors).flat() : [];
    return new ApiError(errors.join(" ") || problem.detail || problem.title, response.status);
  } catch {
    return new ApiError(`La solicitud falló (${response.status}).`, response.status);
  }
}

async function request<T>(path: string, init: RequestInit & { authenticated?: boolean } = {}): Promise<T> {
  const { authenticated, ...options } = init;
  const requestHeaders = new Headers(options.headers);

  if (authenticated) requestHeaders.set("Authorization", `Bearer ${await accessToken()}`);
  if (options.body) requestHeaders.set("Content-Type", "application/json");

  const response = await fetch(new URL(path, baseUrl), { ...options, headers: requestHeaders, cache: "no-store" });

  if (!response.ok) throw await toApiError(response);

  return response.status === 204 ? (undefined as T) : response.json();
}

function query(values: Record<string, string | number | undefined>) {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(values)) if (value !== undefined && value !== "") params.set(key, String(value));
  return params.toString();
}

export const listCatalog = (search: string | undefined, operation: string | undefined, page: number) =>
  request<PagedResult<ListingSummary>>(`/api/catalog?${query({ search, operation, page, pageSize: 12 })}`);

export const getCatalogListing = (id: string) => request<Listing>(`/api/catalog/${id}`);

export const getListing = (id: string) => request<Listing>(`/api/listings/${id}`, { authenticated: true });

export async function getMyVisitor() {
  try {
    return await request<Visitor>("/api/visitors/me", { authenticated: true });
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

export const completeMyProfile = (profile: {
  firstNames: string;
  lastNames: string;
  documentType: number;
  documentNumber: string;
  email: string;
  phone: string;
}) => request<void>("/api/visitors/me/profile", { method: "PUT", body: JSON.stringify(profile), authenticated: true });

const post = <T>(path: string, body: unknown) =>
  request<T>(path, { method: "POST", body: JSON.stringify(body), authenticated: true });

export const listMyVisits = (page: number) =>
  request<PagedResult<Visit>>(`/api/visits?${query({ page, pageSize: 20 })}`, { authenticated: true });

export const requestVisit = (listingId: string, slots: string[]) =>
  post<{ id: string }>("/api/visits", { listingId, slots });

export const counterProposeVisit = (visitId: string, slots: string[]) =>
  post<void>(`/api/visits/${visitId}/counter-proposal`, { slots });

export const scheduleVisit = (visitId: string, startsAt: string) =>
  post<void>(`/api/visits/${visitId}/schedule`, { startsAt });

export const cancelVisit = (visitId: string, reason: string) =>
  post<void>(`/api/visits/${visitId}/cancel`, { reason });

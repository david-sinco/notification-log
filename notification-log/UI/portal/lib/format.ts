import type { VisitorActivity, VisitHistoryEntry } from "@/lib/types";

const operations: Record<string, string> = { Rent: "Arriendo", Sale: "Venta" };

const propertyTypes: Record<string, string> = { Apartment: "Apartamento", Studio: "Apartaestudio" };

export type Tone = "lime" | "warning" | "success" | "info" | "neutral" | "danger";

const visitStatuses: Record<string, { label: string; tone: Tone }> = {
  AwaitingHost: { label: "Esperando al propietario", tone: "warning" },
  AwaitingVisitor: { label: "Te toca responder", tone: "lime" },
  Scheduled: { label: "Agendada", tone: "success" },
  Expired: { label: "Vencida", tone: "neutral" },
  Cancelled: { label: "Cancelada", tone: "neutral" },
  Completed: { label: "Realizada", tone: "info" },
  NoShow: { label: "Inasistencia", tone: "danger" },
};

export const visitRules = {
  maxSlots: 3,
  firstStartHour: 7,
  lastStartHour: 18,
  minLeadHours: 24,
  maxLeadDays: 14,
  respondWithinHours: 48,
  lateCancellationHours: 12,
};

export const bogotaOffsetHours = -5;

export const operationLabel = (operation: string) => operations[operation] ?? operation;

export const propertyTypeLabel = (type: string | null) => (type ? propertyTypes[type] ?? type : "Inmueble");

export const visitStatus = (status: string) => visitStatuses[status] ?? { label: status, tone: "neutral" as Tone };

export const place = (neighborhood: string | null, city: string | null) =>
  [neighborhood, city].filter(Boolean).join(", ") || "Ubicación por confirmar";

export const listingTitle = (type: string | null, neighborhood: string | null) =>
  neighborhood ? `${propertyTypeLabel(type)} en ${neighborhood}` : propertyTypeLabel(type);

export const money = (value: number | null) =>
  value === null
    ? "Precio a convenir"
    : `$ ${new Intl.NumberFormat("es-CO", { maximumFractionDigits: 0 }).format(value)}`;

export const initials = (name: string) =>
  name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word[0].toUpperCase())
    .join("");

const bogotaParts = (value: string | number | Date, options: Intl.DateTimeFormatOptions) =>
  Object.fromEntries(
    new Intl.DateTimeFormat("es-CO", { timeZone: "America/Bogota", ...options })
      .formatToParts(new Date(value))
      .map((part) => [part.type, part.value.replace(".", "")]),
  );

export const shortDate = (value: string | number | Date) => {
  const parts = bogotaParts(value, { weekday: "short", day: "numeric", month: "short" });
  return `${parts.weekday} ${parts.day} ${parts.month}`;
};

export const longDate = (value: string | number | Date) => {
  const parts = bogotaParts(value, { day: "numeric", month: "short", year: "numeric" });
  return `${parts.day} ${parts.month} ${parts.year}`;
};

export const clock = (value: string | number | Date) => {
  const parts = bogotaParts(value, { hour: "2-digit", minute: "2-digit", hourCycle: "h23" });
  return `${parts.hour}:${parts.minute}`;
};

export const shortDateTime = (value: string | number | Date) => `${shortDate(value)}, ${clock(value)}`;

export const slotRange = (start: string | number | Date) =>
  `${clock(start)} – ${clock(new Date(start).getTime() + 60 * 60 * 1000)}`;

export const dayParts = (value: string | number | Date) =>
  bogotaParts(value, { weekday: "short", day: "numeric", month: "short" });

export const historyText = (entry: VisitHistoryEntry, hostName: string) => {
  const mine = entry.by === "Visitor";
  const who = entry.by === "Host" ? hostName : "Llave";

  switch (entry.action) {
    case "Requested":
      return "Pediste la visita";
    case "CounterProposed":
      return mine ? "Propusiste otros horarios" : `${who} propuso otros horarios`;
    case "Scheduled":
      return mine ? "Aceptaste un horario" : `${who} aceptó tu horario`;
    case "Cancelled":
      return mine ? "Cancelaste la visita" : `${who} canceló la visita`;
    case "Expired":
      return "Venció sin respuesta";
    case "Completed":
      return `${who} marcó la visita como realizada`;
    case "NoShow":
      return `${who} registró tu inasistencia`;
    default:
      return entry.action;
  }
};

export const activityText = (activity: VisitorActivity) => {
  const mine = activity.by === "Visitor";
  const host = activity.hostName ?? "El propietario";

  switch (activity.action) {
    case "Registered":
      return "Creaste tu cuenta";
    case "NameChanged":
      return "Cambiaste tu nombre";
    case "EmailChanged":
      return "Confirmaste tu correo";
    case "PhoneChanged":
      return "Confirmaste tu celular";
    case "Requested":
      return "Pediste una visita";
    case "CounterProposed":
      return mine ? "Propusiste otros horarios" : `${host} propuso otros horarios`;
    case "Scheduled":
      return mine ? "Aceptaste un horario" : `${host} aceptó tu horario`;
    case "Cancelled":
      return mine ? "Cancelaste una visita" : `${host} canceló la visita`;
    case "Expired":
      return "La visita venció sin respuesta";
    case "Completed":
      return "Visita realizada";
    case "NoShow":
      return "Registrada como inasistencia";
    default:
      return activity.action;
  }
};

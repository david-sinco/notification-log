const operations: Record<string, string> = { Rent: "Arriendo", Sale: "Venta" };

const propertyTypes: Record<string, string> = { Apartment: "Apartamento", Studio: "Apartaestudio" };

const visitStatuses: Record<string, { label: string; tone: string }> = {
  Requested: { label: "Solicitada", tone: "warning" },
  Confirmed: { label: "Confirmada", tone: "success" },
  Declined: { label: "Rechazada", tone: "danger" },
  Expired: { label: "Vencida", tone: "neutral" },
  Cancelled: { label: "Cancelada", tone: "neutral" },
  Completed: { label: "Realizada", tone: "info" },
  NoShow: { label: "Inasistencia", tone: "danger" },
};

export const documentTypes = [
  { value: 0, name: "CitizenshipCard", label: "Cédula de ciudadanía" },
  { value: 1, name: "ForeignerId", label: "Cédula de extranjería" },
  { value: 2, name: "Passport", label: "Pasaporte" },
];

export const operationLabel = (operation: string) => operations[operation] ?? operation;

export const propertyTypeLabel = (type: string | null) => (type ? propertyTypes[type] ?? type : "Inmueble");

export const visitStatus = (status: string) => visitStatuses[status] ?? { label: status, tone: "neutral" };

export const documentTypeLabel = (name: string | null) =>
  documentTypes.find((type) => type.name === name)?.label ?? name ?? "—";

export const place = (neighborhood: string | null, city: string | null) =>
  [neighborhood, city].filter(Boolean).join(", ") || "Ubicación por confirmar";

export const money = (value: number | null) =>
  value === null
    ? "Precio a convenir"
    : new Intl.NumberFormat("es-CO", { style: "currency", currency: "COP", maximumFractionDigits: 0 }).format(value);

export const dateTime = (value: string) =>
  new Intl.DateTimeFormat("es-CO", { dateStyle: "medium", timeStyle: "short", timeZone: "America/Bogota" }).format(
    new Date(value),
  );

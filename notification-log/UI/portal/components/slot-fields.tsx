"use client";

import { visitRules } from "@/lib/format";

export const slotsToIso = (slots: string[]) => slots.filter(Boolean).map((slot) => new Date(slot).toISOString());

export const slotsHint = `Hasta ${visitRules.maxSlots} horarios de una hora, entre las ${visitRules.earliestHour}:00 y las ${visitRules.latestHour}:00 (el último empieza a las ${visitRules.latestHour - 1}:00), con al menos ${visitRules.minLeadHours} horas de anticipación y máximo ${visitRules.maxLeadDays} días.`;

export function SlotFields({
  id,
  slots,
  onChange,
}: {
  id: string;
  slots: string[];
  onChange: (slots: string[]) => void;
}) {
  const update = (index: number, value: string) => onChange(slots.map((slot, i) => (i === index ? value : slot)));

  return (
    <>
      {slots.map((slot, index) => (
        <div className="field" key={index}>
          <label htmlFor={`${id}-${index}`}>Opción {index + 1}</label>
          <input
            id={`${id}-${index}`}
            className="input"
            type="datetime-local"
            required={index === 0}
            value={slot}
            onChange={(event) => update(index, event.target.value)}
          />
        </div>
      ))}

      {slots.length < visitRules.maxSlots && (
        <button type="button" className="btn btn-secondary btn-sm" onClick={() => onChange([...slots, ""])}>
          Agregar otro horario
        </button>
      )}
    </>
  );
}

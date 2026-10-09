"use client";

import { useState } from "react";
import { bogotaOffsetHours, dayParts, shortDate, shortDateTime, slotRange, visitRules } from "@/lib/format";

const hour = 60 * 60 * 1000;

function buildDays(now: number) {
  const local = new Date(now + bogotaOffsetHours * hour);
  const earliest = now + visitRules.minLeadHours * hour;
  const latest = now + visitRules.maxLeadDays * 24 * hour;

  return Array.from({ length: visitRules.maxLeadDays }, (_, index) => {
    const midnight =
      Date.UTC(local.getUTCFullYear(), local.getUTCMonth(), local.getUTCDate() + index + 1) - bogotaOffsetHours * hour;
    const hours = Array.from({ length: visitRules.lastStartHour - visitRules.firstStartHour + 1 }, (_, offset) => {
      const startsAt = midnight + (visitRules.firstStartHour + offset) * hour;
      return { startsAt, iso: new Date(startsAt).toISOString(), valid: startsAt >= earliest && startsAt <= latest };
    });
    return { key: midnight, parts: dayParts(midnight + 12 * hour), hours };
  });
}

export const respondByPreview = (now: number, slots: string[]) => {
  if (slots.length === 0) return null;
  const earliest = Math.min(...slots.map((slot) => new Date(slot).getTime()));
  return shortDateTime(Math.min(now + visitRules.respondWithinHours * hour, earliest - visitRules.minLeadHours * hour));
};

export const sortSlots = (slots: string[]) =>
  [...slots].sort((a, b) => new Date(a).getTime() - new Date(b).getTime());

export function SlotPicker({
  now,
  slots,
  onChange,
}: {
  now: number;
  slots: string[];
  onChange: (slots: string[]) => void;
}) {
  const [days] = useState(() => buildDays(now));
  const [dayKey, setDayKey] = useState(() => days.find((day) => day.hours.some((h) => h.valid))?.key ?? days[0].key);
  const day = days.find((candidate) => candidate.key === dayKey) ?? days[0];
  const full = slots.length >= visitRules.maxSlots;

  const toggle = (iso: string) =>
    onChange(slots.includes(iso) ? slots.filter((slot) => slot !== iso) : sortSlots([...slots, iso]));

  return (
    <div className="stack">
      <div className="stack" style={{ "--gap": "8px" } as React.CSSProperties}>
        <p className="picker-label">Día</p>
        <div className="day-strip" role="group" aria-label="Día">
          {days.map((candidate) => (
            <button
              key={candidate.key}
              type="button"
              className="day"
              aria-pressed={candidate.key === dayKey}
              aria-label={`${candidate.parts.weekday} ${candidate.parts.day} ${candidate.parts.month}`}
              disabled={!candidate.hours.some((h) => h.valid)}
              onClick={() => setDayKey(candidate.key)}
            >
              <span className="day-weekday">{candidate.parts.weekday}</span>
              <span className="day-number">{candidate.parts.day}</span>
            </button>
          ))}
        </div>
      </div>

      <div className="stack" style={{ "--gap": "8px" } as React.CSSProperties}>
        <p className="picker-label">Hora de inicio · {shortDate(day.key + 12 * hour)}</p>
        <div className="hour-grid" role="group" aria-label="Hora de inicio">
          {day.hours.map((h) => {
            const taken = slots.includes(h.iso);
            return (
              <button
                key={h.iso}
                type="button"
                className="hour"
                aria-pressed={taken}
                disabled={!h.valid || (full && !taken)}
                onClick={() => toggle(h.iso)}
              >
                {String(new Date(h.startsAt + bogotaOffsetHours * hour).getUTCHours()).padStart(2, "0")}:00
              </button>
            );
          })}
        </div>
      </div>

      <div className="stack" style={{ "--gap": "6px" } as React.CSSProperties}>
        <p className="picker-label">
          Tus horarios{" "}
          <span className="count">
            {slots.length}/{visitRules.maxSlots}
          </span>
        </p>
        {slots.length === 0 ? (
          <p className="slot-empty">Toca un día y una hora para agregarlos.</p>
        ) : (
          slots.map((slot) => (
            <div className="slot-item" key={slot}>
              <span>
                {shortDate(slot)} · {slotRange(slot)}
              </span>
              <button
                type="button"
                className="icon-btn"
                aria-label={`Quitar ${shortDateTime(slot)}`}
                onClick={() => toggle(slot)}
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
                  <path d="M6 6l12 12M18 6L6 18" />
                </svg>
              </button>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

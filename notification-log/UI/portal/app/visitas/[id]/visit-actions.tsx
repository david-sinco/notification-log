"use client";

import { useState, useTransition } from "react";
import { cancelVisitAction, counterProposeVisitAction, scheduleVisitAction } from "@/app/actions";
import { respondByPreview, SlotPicker } from "@/components/slot-picker";
import { dayParts, slotRange } from "@/lib/format";
import type { ActionResult } from "@/lib/types";

type Mode = "idle" | "counter" | "cancel";

export function VisitActions({
  visitId,
  now,
  counterpart,
  canRespond,
  canCancel,
  proposedSlots,
  lateCancellation,
}: {
  visitId: string;
  now: number;
  counterpart: string;
  canRespond: boolean;
  canCancel: boolean;
  proposedSlots: string[];
  lateCancellation: boolean;
}) {
  const [mode, setMode] = useState<Mode>("idle");
  const [chosen, setChosen] = useState(proposedSlots[0]);
  const [slots, setSlots] = useState<string[]>([]);
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string>();
  const [pending, startTransition] = useTransition();

  const run = (action: () => Promise<ActionResult>) =>
    startTransition(async () => {
      const result = await action();
      setError(result.error);
      if (!result.error) {
        setMode("idle");
        setSlots([]);
        setReason("");
      }
    });

  const go = (next: Mode) => {
    setError(undefined);
    setMode(next);
  };

  const errorBox = error && (
    <p className="alert" role="alert">
      {error}
    </p>
  );

  const chosenParts = chosen ? dayParts(chosen) : null;

  return (
    <>
      {canRespond && mode !== "counter" && (
        <section className="card" style={{ "--gap": "14px" } as React.CSSProperties}>
          <div className="stack" style={{ "--gap": "4px" } as React.CSSProperties}>
            <h2 className="h2">{counterpart} te propuso estos horarios</h2>
            <p className="small muted">Elige uno para agendar la visita. Cada visita dura una hora.</p>
          </div>
          <div className="stack" role="radiogroup" aria-label="Horarios propuestos" style={{ "--gap": "8px" } as React.CSSProperties}>
            {proposedSlots.map((slot) => {
              const parts = dayParts(slot);
              return (
                <button
                  key={slot}
                  type="button"
                  role="radio"
                  className="option"
                  aria-checked={slot === chosen}
                  onClick={() => setChosen(slot)}
                >
                  <span className="option-radio" />
                  <span className="option-text">
                    <span style={{ fontWeight: 600 }}>
                      {parts.weekday} {parts.day} de {parts.month}
                    </span>
                    <span className="mono small muted">{slotRange(slot)}</span>
                  </span>
                </button>
              );
            })}
          </div>
          {mode === "idle" && errorBox}
          <div className="row">
            <button
              type="button"
              className="btn btn-ink"
              disabled={pending || !chosen}
              onClick={() => run(() => scheduleVisitAction(visitId, chosen))}
            >
              {pending ? "Agendando…" : `Aceptar ${chosenParts ? `${chosenParts.weekday} ${chosenParts.day}` : ""}`}
            </button>
            <button type="button" className="btn" disabled={pending} onClick={() => go("counter")}>
              Ninguno me sirve, proponer otros
            </button>
          </div>
        </section>
      )}

      {canRespond && mode === "counter" && (
        <form
          className="card card-focus"
          onSubmit={(event) => {
            event.preventDefault();
            run(() => counterProposeVisitAction(visitId, slots));
          }}
        >
          <div className="stack" style={{ "--gap": "4px" } as React.CSSProperties}>
            <h2 className="h2">Propón otros horarios</h2>
            <p className="small muted">
              Hasta 3, entre 7:00 y 18:00, desde mañana y hasta dentro de 14 días. Luego le toca responder a{" "}
              {counterpart}.
            </p>
          </div>
          <SlotPicker now={now} slots={slots} onChange={setSlots} />
          {slots.length > 0 && (
            <p className="note">
              {counterpart} tendrá hasta el <strong>{respondByPreview(now, slots)}</strong> para responder.
            </p>
          )}
          {errorBox}
          <div className="row">
            <button className="btn btn-ink" disabled={pending || slots.length === 0}>
              {pending ? "Enviando…" : "Enviar contrapropuesta"}
            </button>
            <button type="button" className="btn" onClick={() => go("idle")}>
              Volver
            </button>
          </div>
        </form>
      )}

      {canCancel && mode === "cancel" && (
        <form
          className="card card-alert"
          style={{ "--gap": "12px" } as React.CSSProperties}
          onSubmit={(event) => {
            event.preventDefault();
            run(() => cancelVisitAction(visitId, reason.trim()));
          }}
        >
          <h2 className="h2">Cancelar la visita</h2>
          {lateCancellation && (
            <p className="note">Faltan menos de 12 horas: quedará registrada como cancelación tardía.</p>
          )}
          <div className="field">
            <label className="label" htmlFor="reason">
              Cuéntale a {counterpart} por qué <span className="required">*</span>
            </label>
            <textarea
              id="reason"
              className="textarea"
              maxLength={500}
              required
              placeholder="Ej.: Ya encontré otro apartamento."
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            />
            <p className="count" style={{ textAlign: "right" }}>
              {reason.length}/500
            </p>
          </div>
          {errorBox}
          <div className="row">
            <button className="btn btn-danger" disabled={pending || reason.trim().length === 0}>
              {pending ? "Cancelando…" : "Cancelar visita"}
            </button>
            <button type="button" className="btn" onClick={() => go("idle")}>
              No, mantenerla
            </button>
          </div>
        </form>
      )}

      {canCancel && mode === "idle" && (
        <button type="button" className="btn btn-text" onClick={() => go("cancel")}>
          Cancelar esta visita
        </button>
      )}
    </>
  );
}

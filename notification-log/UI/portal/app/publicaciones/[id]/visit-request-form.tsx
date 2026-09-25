"use client";

import { useState, useTransition } from "react";
import { requestVisitAction } from "@/app/actions";

const maxSlots = 3;

export function VisitRequestForm({ listingId }: { listingId: string }) {
  const [slots, setSlots] = useState([""]);
  const [error, setError] = useState<string>();
  const [pending, startTransition] = useTransition();

  const update = (index: number, value: string) => setSlots(slots.map((slot, i) => (i === index ? value : slot)));

  const submit = (event: React.FormEvent) => {
    event.preventDefault();
    const starts = slots.filter(Boolean).map((slot) => new Date(slot).toISOString());

    startTransition(async () => {
      const result = await requestVisitAction(listingId, starts);
      setError(result?.error);
    });
  };

  return (
    <form className="stack" onSubmit={submit}>
      <div>
        <h2 className="section-title">Pedir una visita</h2>
        <p className="form-hint">Propón hasta {maxSlots} horarios. El propietario confirmará uno; cada visita dura una hora.</p>
      </div>

      {slots.map((slot, index) => (
        <div className="field" key={index}>
          <label htmlFor={`slot-${index}`}>Opción {index + 1}</label>
          <input
            id={`slot-${index}`}
            className="input"
            type="datetime-local"
            required={index === 0}
            value={slot}
            onChange={(event) => update(index, event.target.value)}
          />
        </div>
      ))}

      {slots.length < maxSlots && (
        <button type="button" className="btn btn-secondary btn-sm" onClick={() => setSlots([...slots, ""])}>
          Agregar otro horario
        </button>
      )}

      {error && <div className="alert alert-danger">{error}</div>}

      <button className="btn btn-primary btn-block" disabled={pending}>
        {pending ? "Enviando…" : "Pedir visita"}
      </button>
    </form>
  );
}

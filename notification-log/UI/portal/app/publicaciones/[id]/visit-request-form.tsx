"use client";

import { useState, useTransition } from "react";
import { requestVisitAction } from "@/app/actions";
import { respondByPreview, SlotPicker } from "@/components/slot-picker";

export function VisitRequestForm({ listingId, now }: { listingId: string; now: number }) {
  const [slots, setSlots] = useState<string[]>([]);
  const [error, setError] = useState<string>();
  const [pending, startTransition] = useTransition();
  const deadline = respondByPreview(now, slots);

  const submit = (event: React.FormEvent) => {
    event.preventDefault();
    startTransition(async () => setError((await requestVisitAction(listingId, slots))?.error));
  };

  return (
    <form className="stack" onSubmit={submit}>
      <div className="stack" style={{ "--gap": "4px" } as React.CSSProperties}>
        <h2 className="h2">Pide una visita</h2>
        <p className="small muted">Elige hasta 3 horarios de una hora. El propietario acepta uno o te propone otros.</p>
      </div>

      <SlotPicker now={now} slots={slots} onChange={setSlots} />

      <div className="note">
        <p>Entre 7:00 y 18:00 · desde mañana hasta dentro de 14 días.</p>
        {deadline && (
          <p>
            El propietario tendrá hasta el <strong>{deadline}</strong> para responder.
          </p>
        )}
      </div>

      {error && (
        <p className="alert" role="alert">
          {error}
        </p>
      )}

      <button className="btn btn-ink btn-block" disabled={pending || slots.length === 0}>
        {pending ? "Enviando…" : "Enviar solicitud"}
      </button>
    </form>
  );
}

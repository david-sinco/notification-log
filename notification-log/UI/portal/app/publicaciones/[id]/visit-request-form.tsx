"use client";

import { useState, useTransition } from "react";
import { requestVisitAction } from "@/app/actions";
import { SlotFields, slotsHint, slotsToIso } from "@/components/slot-fields";

export function VisitRequestForm({ listingId }: { listingId: string }) {
  const [slots, setSlots] = useState([""]);
  const [error, setError] = useState<string>();
  const [pending, startTransition] = useTransition();

  const submit = (event: React.FormEvent) => {
    event.preventDefault();

    startTransition(async () => {
      const result = await requestVisitAction(listingId, slotsToIso(slots));
      setError(result?.error);
    });
  };

  return (
    <form className="stack" onSubmit={submit}>
      <div>
        <h2 className="section-title">Pedir una visita</h2>
        <p className="form-hint">
          {slotsHint} El propietario acepta uno, te propone otros o cancela.
        </p>
      </div>

      <SlotFields id="slot" slots={slots} onChange={setSlots} />

      {error && <div className="alert alert-danger">{error}</div>}

      <button className="btn btn-primary btn-block" disabled={pending}>
        {pending ? "Enviando…" : "Pedir visita"}
      </button>
    </form>
  );
}

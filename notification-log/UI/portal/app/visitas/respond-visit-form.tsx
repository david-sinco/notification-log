"use client";

import { useState, useTransition } from "react";
import { counterProposeVisitAction, scheduleVisitAction } from "@/app/actions";
import { SlotFields, slotsHint, slotsToIso } from "@/components/slot-fields";
import { dateTime } from "@/lib/format";

export function RespondVisitForm({ visitId, proposedSlots }: { visitId: string; proposedSlots: string[] }) {
  const [proposing, setProposing] = useState(false);
  const [slots, setSlots] = useState([""]);
  const [error, setError] = useState<string>();
  const [pending, startTransition] = useTransition();

  const run = (action: () => Promise<{ error?: string }>) =>
    startTransition(async () => setError((await action()).error));

  const submit = (event: React.FormEvent) => {
    event.preventDefault();
    run(() => counterProposeVisitAction(visitId, slotsToIso(slots)));
  };

  return (
    <div className="stack" style={{ minWidth: 280 }}>
      {proposedSlots.map((slot) => (
        <button
          key={slot}
          className="btn btn-primary btn-sm"
          disabled={pending}
          onClick={() => run(() => scheduleVisitAction(visitId, slot))}
        >
          Aceptar {dateTime(slot)}
        </button>
      ))}

      {proposing ? (
        <form className="stack" onSubmit={submit}>
          <span className="form-hint">{slotsHint}</span>
          <SlotFields id={`counter-${visitId}`} slots={slots} onChange={setSlots} />
          <div className="form-actions" style={{ marginTop: 0 }}>
            <button type="button" className="btn btn-secondary btn-sm" onClick={() => setProposing(false)}>
              Volver
            </button>
            <button className="btn btn-primary btn-sm" disabled={pending}>
              {pending ? "Enviando…" : "Proponer"}
            </button>
          </div>
        </form>
      ) : (
        <button className="btn btn-secondary btn-sm" disabled={pending} onClick={() => setProposing(true)}>
          Proponer otros horarios
        </button>
      )}

      {error && <div className="alert alert-danger">{error}</div>}
    </div>
  );
}

"use client";

import { useState, useTransition } from "react";
import { cancelVisitAction } from "@/app/actions";

export function CancelVisitForm({ visitId }: { visitId: string }) {
  const [open, setOpen] = useState(false);
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string>();
  const [pending, startTransition] = useTransition();

  if (!open)
    return (
      <button className="btn btn-danger btn-sm" onClick={() => setOpen(true)}>
        Cancelar visita
      </button>
    );

  const submit = (event: React.FormEvent) => {
    event.preventDefault();
    startTransition(async () => setError((await cancelVisitAction(visitId, reason)).error));
  };

  return (
    <form className="stack" onSubmit={submit} style={{ minWidth: 280 }}>
      <div className="field">
        <label htmlFor={`reason-${visitId}`}>¿Por qué la cancelas?</label>
        <input
          id={`reason-${visitId}`}
          className="input"
          required
          value={reason}
          onChange={(event) => setReason(event.target.value)}
        />
        <span className="form-hint">Cancelar poco antes de la cita cuenta como cancelación tardía.</span>
      </div>
      {error && <div className="alert alert-danger">{error}</div>}
      <div className="form-actions" style={{ marginTop: 0 }}>
        <button type="button" className="btn btn-secondary btn-sm" onClick={() => setOpen(false)}>
          Volver
        </button>
        <button className="btn btn-danger btn-sm" disabled={pending}>
          {pending ? "Cancelando…" : "Confirmar cancelación"}
        </button>
      </div>
    </form>
  );
}

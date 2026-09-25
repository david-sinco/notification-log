"use client";

import { useActionState } from "react";
import { completeProfileAction } from "@/app/actions";
import { documentTypes } from "@/lib/format";

type Defaults = { firstNames: string; lastNames: string; email: string; phone: string };

export function ProfileForm({ next, defaults }: { next: string; defaults: Defaults }) {
  const [state, action, pending] = useActionState(completeProfileAction, {});

  return (
    <form className="surface-card" action={action}>
      <input type="hidden" name="next" value={next} />

      <div className="form-grid">
        <div className="field">
          <label htmlFor="firstNames">Nombres</label>
          <input id="firstNames" name="firstNames" className="input" required defaultValue={defaults.firstNames} />
        </div>
        <div className="field">
          <label htmlFor="lastNames">Apellidos</label>
          <input id="lastNames" name="lastNames" className="input" required defaultValue={defaults.lastNames} />
        </div>
        <div className="field">
          <label htmlFor="documentType">Tipo de documento</label>
          <select id="documentType" name="documentType" className="input" defaultValue={0}>
            {documentTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label htmlFor="documentNumber">Número de documento</label>
          <input id="documentNumber" name="documentNumber" className="input" required />
        </div>
        <div className="field">
          <label htmlFor="email">Correo</label>
          <input id="email" name="email" type="email" className="input" required defaultValue={defaults.email} />
        </div>
        <div className="field">
          <label htmlFor="phone">Celular</label>
          <input id="phone" name="phone" type="tel" className="input" required defaultValue={defaults.phone} />
        </div>
      </div>

      {state.error && (
        <div className="alert alert-danger" style={{ marginTop: "1rem" }}>
          {state.error}
        </div>
      )}

      <div className="form-actions">
        <button className="btn btn-primary" disabled={pending}>
          {pending ? "Guardando…" : "Guardar y continuar"}
        </button>
      </div>
    </form>
  );
}

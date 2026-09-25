import { LoginButton } from "@/components/login-button";

export function SignInRequired({ message }: { message: string }) {
  return (
    <main className="main">
      <div className="container">
        <div className="surface-card empty-state">
          <h2>Inicia sesión</h2>
          <p style={{ marginBottom: "1.25rem" }}>{message}</p>
          <LoginButton className="btn btn-primary">Iniciar sesión o crear cuenta</LoginButton>
        </div>
      </div>
    </main>
  );
}

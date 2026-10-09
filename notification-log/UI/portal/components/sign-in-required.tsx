import { LoginButton } from "@/components/login-button";

export function SignInRequired({ message }: { message: string }) {
  return (
    <main className="main">
      <div className="container container-narrow">
        <div className="empty">
          <h1 className="h2">Ingresa a tu cuenta</h1>
          <p>{message}</p>
          <LoginButton className="btn btn-ink">Ingresar o crear cuenta</LoginButton>
        </div>
      </div>
    </main>
  );
}

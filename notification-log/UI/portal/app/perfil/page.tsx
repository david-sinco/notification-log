import { auth } from "@/auth";
import { SignInRequired } from "@/components/sign-in-required";
import { documentTypeLabel } from "@/lib/format";
import { getMyVisitor } from "@/lib/rentals";
import { ProfileForm } from "./profile-form";

export const metadata = { title: "Mi perfil" };

export default async function ProfilePage({ searchParams }: { searchParams: Promise<{ next?: string }> }) {
  const session = await auth();
  if (!session) return <SignInRequired message="Inicia sesión para ver y completar tus datos." />;

  const { next = "/" } = await searchParams;
  const visitor = await getMyVisitor();

  if (visitor?.status === "Registered")
    return (
      <main className="main">
        <div className="container">
          <header className="page-header">
            <p className="eyebrow">Tu cuenta</p>
            <h1>{visitor.displayName}</h1>
            <p>Estos son los datos con los que pides visitas.</p>
          </header>
          <section className="surface-card">
            <dl className="kv-list">
              <dt>Nombres</dt>
              <dd>{visitor.firstNames}</dd>
              <dt>Apellidos</dt>
              <dd>{visitor.lastNames}</dd>
              <dt>Documento</dt>
              <dd>
                {documentTypeLabel(visitor.documentType)} {visitor.documentNumber}
              </dd>
              <dt>Correo</dt>
              <dd>{visitor.email}</dd>
              <dt>Celular</dt>
              <dd>{visitor.phone}</dd>
            </dl>
          </section>
        </div>
      </main>
    );

  return (
    <main className="main">
      <div className="container" style={{ maxWidth: 760 }}>
        <header className="page-header">
          <p className="eyebrow">Bienvenido a Llave</p>
          <h1>Completa tu perfil</h1>
          <p>Los propietarios ven estos datos cuando pides una visita. Solo tienes que llenarlos una vez.</p>
        </header>
        <ProfileForm
          next={next}
          defaults={{
            firstNames: visitor?.firstNames ?? "",
            lastNames: visitor?.lastNames ?? "",
            email: visitor?.email || session.user.email || "",
            phone: visitor?.phone || session.user.phone || "",
          }}
        />
      </div>
    </main>
  );
}

"use server";

import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import { getToken } from "next-auth/jwt";
import { auth, issuer, signIn, signOut } from "@/auth";
import { ApiError, cancelVisit, completeMyProfile, requestVisit } from "@/lib/rentals";
import type { ActionResult } from "@/lib/types";

export async function login(redirectTo: string) {
  await signIn("llave", { redirectTo });
}

export async function logout() {
  const token = await getToken({
    req: { headers: await headers() },
    secret: process.env.AUTH_SECRET,
    secureCookie: process.env.AUTH_URL?.startsWith("https://") ?? false,
  });

  await signOut({ redirect: false });

  const endSession = new URL("connect/endsession", issuer);
  if (token?.idToken) endSession.searchParams.set("id_token_hint", token.idToken);
  endSession.searchParams.set("post_logout_redirect_uri", new URL("/", process.env.AUTH_URL).href);

  redirect(endSession.href);
}

async function currentUserId() {
  const session = await auth();
  if (!session) throw new ApiError("Tu sesión terminó. Inicia sesión de nuevo.", 401);
  return session.user.id;
}

async function attempt(action: () => Promise<unknown>): Promise<ActionResult> {
  try {
    await action();
    return {};
  } catch (error) {
    return { error: error instanceof ApiError ? error.message : "Ocurrió un error inesperado. Intenta de nuevo." };
  }
}

export async function requestVisitAction(listingId: string, slotStarts: string[]): Promise<ActionResult> {
  const result = await attempt(async () => requestVisit(await currentUserId(), listingId, slotStarts));
  if (result.error) return result;
  redirect("/visitas");
}

export async function cancelVisitAction(visitId: string, reason: string): Promise<ActionResult> {
  const result = await attempt(async () => cancelVisit(await currentUserId(), visitId, reason));
  revalidatePath("/visitas");
  return result;
}

export async function completeProfileAction(_: ActionResult, form: FormData): Promise<ActionResult> {
  const field = (name: string) => String(form.get(name) ?? "").trim();

  const result = await attempt(() =>
    completeMyProfile({
      firstNames: field("firstNames"),
      lastNames: field("lastNames"),
      documentType: Number(field("documentType")),
      documentNumber: field("documentNumber"),
      email: field("email"),
      phone: field("phone"),
    }),
  );

  if (result.error) return result;

  const next = field("next");
  redirect(next.startsWith("/") && !next.startsWith("//") ? next : "/");
}

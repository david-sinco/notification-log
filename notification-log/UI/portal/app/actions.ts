"use server";

import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import { getToken } from "next-auth/jwt";
import { issuer, signIn, signOut } from "@/auth";
import {
  ApiError,
  cancelVisit,
  completeMyProfile,
  counterProposeVisit,
  requestVisit,
  scheduleVisit,
} from "@/lib/rentals";
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

async function attempt(action: () => Promise<unknown>): Promise<ActionResult> {
  try {
    await action();
    return {};
  } catch (error) {
    return { error: error instanceof ApiError ? error.message : "Ocurrió un error inesperado. Intenta de nuevo." };
  }
}

export async function requestVisitAction(listingId: string, slots: string[]): Promise<ActionResult> {
  const result = await attempt(() => requestVisit(listingId, slots));
  if (result.error) return result;
  redirect("/visitas");
}

async function visitAction(action: () => Promise<unknown>): Promise<ActionResult> {
  const result = await attempt(action);
  revalidatePath("/visitas");
  return result;
}

export const cancelVisitAction = async (visitId: string, reason: string) =>
  visitAction(() => cancelVisit(visitId, reason));

export const scheduleVisitAction = async (visitId: string, startsAt: string) =>
  visitAction(() => scheduleVisit(visitId, startsAt));

export const counterProposeVisitAction = async (visitId: string, slots: string[]) =>
  visitAction(() => counterProposeVisit(visitId, slots));

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

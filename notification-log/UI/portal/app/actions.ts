"use server";

import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import { getToken } from "next-auth/jwt";
import { issuer, signIn, signOut } from "@/auth";
import { ApiError, cancelVisit, counterProposeVisit, requestVisit, scheduleVisit } from "@/lib/rentals";
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

const errorMessage = (error: unknown) =>
  error instanceof ApiError ? error.message : "Ocurrió un error inesperado. Intenta de nuevo.";

export async function requestVisitAction(listingId: string, slots: string[]): Promise<ActionResult> {
  let visitId: string;

  try {
    visitId = (await requestVisit(listingId, slots)).id;
  } catch (error) {
    return { error: errorMessage(error) };
  }

  revalidatePath("/", "layout");
  redirect(`/visitas/${visitId}`);
}

async function visitAction(action: () => Promise<unknown>): Promise<ActionResult> {
  try {
    await action();
  } catch (error) {
    return { error: errorMessage(error) };
  }

  revalidatePath("/", "layout");
  return {};
}

export const cancelVisitAction = async (visitId: string, reason: string) =>
  visitAction(() => cancelVisit(visitId, reason));

export const scheduleVisitAction = async (visitId: string, startsAt: string) =>
  visitAction(() => scheduleVisit(visitId, startsAt));

export const counterProposeVisitAction = async (visitId: string, slots: string[]) =>
  visitAction(() => counterProposeVisit(visitId, slots));

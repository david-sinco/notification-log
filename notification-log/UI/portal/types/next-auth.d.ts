import type { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface Session {
    user: { id: string; role?: string; phone?: string } & DefaultSession["user"];
  }
}

declare module "@auth/core/jwt" {
  interface JWT {
    userId?: string;
    role?: string;
    phone?: string;
    accessToken?: string;
    idToken?: string;
    expiresAt?: number;
  }
}

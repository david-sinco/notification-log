import NextAuth from "next-auth";

export const issuer = new URL(process.env.IDENTITY_ISSUER ?? "https://localhost:7191").href;

export const { handlers, auth, signIn, signOut } = NextAuth({
  providers: [
    {
      id: "llave",
      name: "Llave",
      type: "oidc",
      issuer,
      clientId: process.env.PORTAL_CLIENT_ID,
      clientSecret: process.env.PORTAL_CLIENT_SECRET,
      checks: ["pkce", "state"],
      authorization: { params: { scope: "openid email phone roles rentals" } },
      token: new URL("connect/token", issuer).href,
    },
  ],
  callbacks: {
    jwt({ token, account, profile }) {
      if (account) {
        return {
          ...token,
          phone: typeof profile?.phone_number === "string" ? profile.phone_number : undefined,
          accessToken: account.access_token,
          idToken: account.id_token,
          expiresAt: account.expires_at,
        };
      }

      return token.expiresAt && Date.now() < token.expiresAt * 1000 ? token : null;
    },
    session({ session, token }) {
      session.user.id = token.sub ?? "";
      session.user.phone = token.phone;
      return session;
    },
  },
});

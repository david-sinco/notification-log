"use client";

import { usePathname, useSearchParams } from "next/navigation";
import { login } from "@/app/actions";

export function LoginButton({ className, children }: { className: string; children: React.ReactNode }) {
  const pathname = usePathname();
  const search = useSearchParams().toString();

  return (
    <form action={login.bind(null, search ? `${pathname}?${search}` : pathname)}>
      <button className={className}>{children}</button>
    </form>
  );
}

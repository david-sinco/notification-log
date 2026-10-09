"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

export function NavLink({ href, match, children }: { href: string; match: string[]; children: React.ReactNode }) {
  const pathname = usePathname();
  const active = match.some((prefix) => (prefix === "/" ? pathname === "/" : pathname.startsWith(prefix)));

  return (
    <Link className="nav-link" href={href} aria-current={active ? "page" : undefined}>
      {children}
    </Link>
  );
}

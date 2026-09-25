import type { Metadata } from "next";
import { Fraunces, Inter } from "next/font/google";
import { SiteHeader } from "@/components/site-header";
import "./globals.css";

const inter = Inter({ subsets: ["latin"], variable: "--font-inter" });
const fraunces = Fraunces({ subsets: ["latin"], weight: "600", variable: "--font-fraunces" });

export const metadata: Metadata = {
  title: { default: "Llave · Arriendos y ventas", template: "%s · Llave" },
  description: "Encuentra apartamentos en arriendo y venta y agenda visitas con sus propietarios.",
  icons: { icon: "/llave-mark.svg" },
};

export const viewport = { themeColor: "#16302e" };

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es" className={`${inter.variable} ${fraunces.variable}`}>
      <body>
        <SiteHeader />
        {children}
        <footer className="site-footer">
          <div className="container">© {new Date().getFullYear()} Llave · Portal inmobiliario</div>
        </footer>
      </body>
    </html>
  );
}

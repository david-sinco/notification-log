import type { Metadata } from "next";
import { Bricolage_Grotesque, JetBrains_Mono, Onest } from "next/font/google";
import { SiteHeader } from "@/components/site-header";
import "./globals.css";

const onest = Onest({ subsets: ["latin"], weight: ["400", "500", "600"], variable: "--font-onest" });
const bricolage = Bricolage_Grotesque({ subsets: ["latin"], weight: ["500", "700"], variable: "--font-bricolage" });
const jetbrains = JetBrains_Mono({ subsets: ["latin"], weight: ["400", "500"], variable: "--font-jetbrains" });

export const metadata: Metadata = {
  title: { default: "Llave · Arriendos y ventas", template: "%s · Llave" },
  description: "Encuentra apartamentos en arriendo y venta y agenda visitas con sus propietarios.",
  icons: { icon: "/llave-mark.svg" },
};

export const viewport = { themeColor: "#101a16" };

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es" className={`${onest.variable} ${bricolage.variable} ${jetbrains.variable}`}>
      <body>
        <SiteHeader />
        {children}
        <footer className="site-footer">
          <div className="container">
            <span>© {new Date().getFullYear()} Llave · Portal inmobiliario</span>
            <span className="mono">Precios en pesos · horarios en hora de Bogotá</span>
          </div>
        </footer>
      </body>
    </html>
  );
}

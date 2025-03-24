import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Kërko",
  description: "Kërko personin",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`antialiased min-h-screen bg-[#0a0303]`}
        style={{
          backgroundImage: "url('/alb.png')",
          backgroundRepeat: "no-repeat",
          backgroundPosition: "center",
          backgroundSize: "cover",
          backgroundBlendMode: "soft-light",
          backgroundAttachment: "fixed",
        }}
      >
        {children}
      </body>
    </html>
  );
}

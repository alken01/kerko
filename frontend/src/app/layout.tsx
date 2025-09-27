import { ThemeProvider } from "@/contexts/ThemeContext";
import { Analytics } from "@vercel/analytics/react";
import { SpeedInsights } from "@vercel/speed-insights/next";
import type { Metadata } from "next";
import "./globals.css";

// Define your domain (replace with your actual domain)
const BASE_URL = "https://kerko.vercel.app"; // Update with your production domain

export const metadata: Metadata = {
  title: {
    default: "Kërko - Kërkim në Regjistrat Publikë të Shqipërisë",
    template: "%s | Kërko", // For dynamic page titles
  },
  description:
    "Kërko është një platformë për kërkimin e regjistrave publikë shqiptarë, përfshirë emrat, rrogat dhe targat.",
  keywords: [
    "Kërko",
    "regjistra publikë shqiptarë",
    "platformë kërkimi",
    "kërkim emri",
    "kërkim targe",
    "të dhëna shqiptare",
  ],
  robots: {
    index: true,
    follow: true,
  },
  alternates: {
    canonical: BASE_URL, // Canonical URL for SEO
  },
  icons: {
    icon: "/favicon.ico",
    apple: "/apple-touch-icon.png", // Remove if you don't have this file
  },
  openGraph: {
    title: "Kërko - Kërkim në Regjistrat Publikë të Shqipërisë",
    description:
      "Kërko është një platformë për kërkimin e regjistrave publikë shqiptarë, përfshirë emrat, rrogat dhe targat.",
    url: BASE_URL,
    siteName: "Kërko",
    images: [
      {
        url: `${BASE_URL}/metaimage.jpg`, // Absolute URL for og:image
        width: 1200, // Recommended size
        height: 630,
        alt: "Kërko - Kërkim në Regjistrat Publikë",
      },
    ],
    locale: "sq_AL", // Albanian locale
    type: "website",
  },
  twitter: {
    card: "summary_large_image", // Suitable for large images
    title: "Kërko - Kërkim në Regjistrat Publikë të Shqipërisë",
    description:
      "Kërko është një platformë për kërkimin e regjistrave publikë shqiptarë, përfshirë emrat, rrogat dhe targat.",
    images: [`${BASE_URL}/metaimage.jpg`], // Absolute URL for Twitter
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="sq">
      <body className="antialiased min-h-screen bg-surface-primary text-text-primary">
        <ThemeProvider>{children}</ThemeProvider>
        <Analytics />
        <SpeedInsights />
      </body>
    </html>
  );
}

"use client";

import { createContext, useContext, useState, useCallback, useEffect, ReactNode } from "react";
import sq from "./locales/sq.json";
import en from "./locales/en.json";

export type Locale = "sq" | "en";

const messages: Record<Locale, Record<string, unknown>> = { sq, en };

interface TranslationContextType {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: string) => string;
  tn: (singularKey: string, pluralKey: string, count: number) => string;
}

const TranslationContext = createContext<TranslationContextType | null>(null);

function getNestedValue(obj: unknown, path: string): string {
  const value = path.split(".").reduce<unknown>(
    (acc, part) => (acc && typeof acc === "object" ? (acc as Record<string, unknown>)[part] : undefined),
    obj
  );
  return typeof value === "string" ? value : path;
}

function getInitialLocale(): Locale {
  if (typeof window !== "undefined") {
    const saved = localStorage.getItem("kerko-locale") as Locale | null;
    if (saved && messages[saved]) return saved;
  }
  return "sq";
}

export function TranslationProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(getInitialLocale);

  useEffect(() => {
    document.documentElement.lang = locale;
  }, [locale]);

  const setLocale = useCallback((newLocale: Locale) => {
    setLocaleState(newLocale);
    localStorage.setItem("kerko-locale", newLocale);
  }, []);

  const t = useCallback(
    (key: string) => getNestedValue(messages[locale], key),
    [locale]
  );

  const tn = useCallback(
    (singularKey: string, pluralKey: string, count: number) =>
      count === 1
        ? getNestedValue(messages[locale], singularKey)
        : getNestedValue(messages[locale], pluralKey),
    [locale]
  );

  return (
    <TranslationContext.Provider value={{ locale, setLocale, t, tn }}>
      {children}
    </TranslationContext.Provider>
  );
}

export function useTranslation() {
  const context = useContext(TranslationContext);
  if (!context) throw new Error("useTranslation must be used within TranslationProvider");
  return context;
}

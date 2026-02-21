"use client";

import { useTranslation } from "@/i18n/TranslationContext";
import { cn } from "@/lib/utils";

export function LanguageSwitcher() {
  const { locale, setLocale } = useTranslation();

  return (
    <button
      onClick={() => setLocale(locale === "sq" ? "en" : "sq")}
      className={cn(
        "px-2 py-1 rounded-md text-xs font-semibold",
        "bg-surface-secondary border border-border-semantic-secondary",
        "text-text-secondary hover:text-text-primary hover:bg-surface-interactive",
        "transition-all duration-200"
      )}
      title={locale === "sq" ? "Switch to English" : "Shko në Shqip"}
    >
      {locale === "sq" ? "EN" : "SQ"}
    </button>
  );
}

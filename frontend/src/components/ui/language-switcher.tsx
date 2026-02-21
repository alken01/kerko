"use client";

import { useTranslation } from "@/i18n/TranslationContext";
import { Button } from "@/components/ui/button";

export function LanguageSwitcher() {
  const { locale, setLocale } = useTranslation();

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={() => setLocale(locale === "sq" ? "en" : "sq")}
      className="h-9 w-9 bg-surface-interactive hover:bg-surface-interactive-hover border-2 border-border-semantic-secondary transition-colors text-xs font-semibold text-text-primary"
      title={locale === "sq" ? "Switch to English" : "Shko në Shqip"}
    >
      {locale === "sq" ? "EN" : "SQ"}
    </Button>
  );
}

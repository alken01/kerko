"use client";

import { useOnlineStatus } from "@/hooks/useOnlineStatus";
import { useTranslation } from "@/i18n/TranslationContext";
import { WifiOff } from "lucide-react";

interface OfflineIndicatorProps {
  onShowSaved?: () => void;
}

export function OfflineIndicator({ onShowSaved }: OfflineIndicatorProps) {
  const { t } = useTranslation();
  const isOnline = useOnlineStatus();

  if (isOnline) {
    return null;
  }

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 bg-amber-500 text-amber-950 py-2 px-4 flex items-center justify-center gap-2 text-sm font-medium">
      <WifiOff className="h-4 w-4" />
      <span>{t("offline.youAreOffline")}</span>
      {onShowSaved && (
        <>
          <span>-</span>
          <button
            onClick={onShowSaved}
            className="underline hover:no-underline font-semibold"
          >
            {t("offline.showSaved")}
          </button>
        </>
      )}
    </div>
  );
}

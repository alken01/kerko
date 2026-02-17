"use client";

import { useState, useCallback, useEffect } from "react";
import { Bookmark, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { useSavedItems } from "@/contexts/SavedItemsContext";
import { useToast } from "@/components/ui/toast";
import { useTranslation } from "@/i18n/TranslationContext";
import { SavedItemType, generateItemKey, generateDisplayName } from "@/types/saved";
import {
  PatronazhistResponse,
  PersonResponse,
  RrogatResponse,
  TargatResponse,
} from "@/types/kerko";

interface SaveButtonProps {
  type: SavedItemType;
  data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse;
  className?: string;
}

export function SaveButton({ type, data, className }: SaveButtonProps) {
  const { t } = useTranslation();
  const { isItemSaved, saveItem, removeItemByKey, savedCount } = useSavedItems();
  const { showToast } = useToast();
  const [isLoading, setIsLoading] = useState(false);
  const [hasShownPWAHint, setHasShownPWAHint] = useState(false);

  const isSaved = isItemSaved(type, data);

  // Check if we've shown the PWA hint before
  useEffect(() => {
    const shown = localStorage.getItem("pwa-hint-shown");
    setHasShownPWAHint(shown === "true");
  }, []);

  const handleClick = useCallback(
    async (e: React.MouseEvent) => {
      e.stopPropagation();
      setIsLoading(true);

      try {
        if (isSaved) {
          const key = generateItemKey(type, data);
          await removeItemByKey(key);
          showToast(t("saved.removedFromSaved"), "removed");
        } else {
          const result = await saveItem(type, data);
          if (result) {
            const displayName = generateDisplayName(type, data);
            // Show PWA hint on first save or every 5th save
            const showPWAHint = !hasShownPWAHint || (savedCount + 1) % 5 === 0;
            showToast(`${displayName} ${t("saved.wasSaved")}`, "saved", showPWAHint);

            if (!hasShownPWAHint) {
              localStorage.setItem("pwa-hint-shown", "true");
              setHasShownPWAHint(true);
            }
          }
        }
      } catch (error) {
        console.error("Failed to save/remove item:", error);
      } finally {
        setIsLoading(false);
      }
    },
    [isSaved, type, data, saveItem, removeItemByKey, showToast, hasShownPWAHint, savedCount, t]
  );

  return (
    <button
      onClick={handleClick}
      disabled={isLoading}
      className={cn(
        "p-1.5 rounded-md transition-all duration-200",
        "hover:bg-surface-interactive focus:outline-none focus:ring-2 focus:ring-border-semantic-primary",
        isSaved
          ? "text-amber-500 hover:text-amber-600"
          : "text-text-tertiary hover:text-text-secondary",
        className
      )}
      aria-label={isSaved ? t("saved.removeFromSaved") : t("saved.save")}
    >
      {isLoading ? (
        <Loader2 className="h-5 w-5 animate-spin" />
      ) : (
        <Bookmark
          className={cn("h-5 w-5", isSaved && "fill-current")}
        />
      )}
    </button>
  );
}

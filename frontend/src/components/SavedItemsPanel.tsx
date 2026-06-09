"use client";

import { useState, useRef } from "react";
import { Bookmark, X } from "lucide-react";
import { useSavedItems } from "@/contexts/SavedItemsContext";
import { useTranslation } from "@/i18n/TranslationContext";
import { SavedItemsList } from "./SavedItemsList";
import { cn } from "@/lib/utils";

export function SavedItemsPanel() {
  const { t, tn } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [isClosing, setIsClosing] = useState(false);
  const { savedCount, storageError } = useSavedItems();

  const touchStartY = useRef<number | null>(null);
  const panelRef = useRef<HTMLDivElement>(null);

  const handleTouchStart = (e: React.TouchEvent) => {
    touchStartY.current = e.touches[0].clientY;
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (touchStartY.current === null) return;
    const delta = e.changedTouches[0].clientY - touchStartY.current;
    if (delta > 60) handleClose();
    touchStartY.current = null;
  };

  const handleClose = () => {
    setIsClosing(true);
    setTimeout(() => {
      setIsOpen(false);
      setIsClosing(false);
    }, 300);
  };

  return (
    <>
      {/* Floating Button */}
      <button
        onClick={() => setIsOpen(true)}
        className={cn(
          "fixed bottom-5 right-5 z-40",
          "flex items-center justify-center",
          "w-12 h-12 rounded-xl",
          "bg-surface-secondary border border-border-semantic-primary",
          "transition-all duration-200 active:scale-95",
          "hover:bg-surface-interactive"
        )}
      >
        <Bookmark className="h-5 w-5 text-accent-saved fill-current" />
        {savedCount > 0 && (
          <span className="absolute -top-1.5 -right-1.5 min-w-5 h-5 flex items-center justify-center bg-accent-saved text-white text-xs font-bold rounded-full">
            {savedCount}
          </span>
        )}
      </button>

      {/* Backdrop + Panel */}
      {isOpen && (
        <>
          {/* Backdrop */}
          <div
            className={cn(
              "fixed inset-0 z-50 bg-black/40 duration-300",
              isClosing ? "animate-out fade-out" : "animate-in fade-in"
            )}
            onClick={handleClose}
          />

          {/* Panel — bottom sheet on mobile, side drawer on desktop */}
          <div
            ref={panelRef}
            onTouchStart={handleTouchStart}
            onTouchEnd={handleTouchEnd}
            className={cn(
            "fixed z-50 bg-surface-primary flex flex-col overflow-hidden",
            "border-border-semantic-secondary",
            // Mobile: slide up from bottom
            "inset-x-0 bottom-0 max-h-[85vh] rounded-t-2xl border-t-2",
            // Desktop: side drawer from right
            "md:inset-y-0 md:right-0 md:left-auto md:bottom-auto md:w-[460px] md:max-h-full md:rounded-none md:rounded-l-2xl md:border-t-0 md:border-l-2",
            "duration-300",
            isClosing
              ? "animate-out slide-out-to-bottom md:slide-out-to-right"
              : "animate-in slide-in-from-bottom md:slide-in-from-right"
          )}>
            {/* Drag handle (mobile only) */}
            <div className="flex-shrink-0 flex justify-center pt-3 pb-1 md:hidden">
              <div className="w-10 h-1 rounded-full bg-border-semantic-secondary" />
            </div>

            {/* Header */}
            <div className="flex-shrink-0 border-b border-border-semantic-secondary px-4 md:px-6 py-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Bookmark className="h-6 w-6 text-accent-saved fill-current" />
                  <div>
                    <h2 className="text-lg font-semibold text-text-primary">
                      {t("saved.savedItems")}
                    </h2>
                    {savedCount > 0 && (
                      <p className="text-xs text-text-secondary">
                        {savedCount} {tn("saved.result", "saved.results", savedCount)}
                      </p>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <button
                    onClick={handleClose}
                    className="w-10 h-10 rounded-xl flex items-center justify-center hover:bg-surface-interactive transition-colors"
                  >
                    <X className="h-5 w-5 text-text-secondary" />
                  </button>
                </div>
              </div>
            </div>

            {/* Content */}
            <div className="flex-1 overflow-auto">
              <div className="px-4 md:px-6 py-6 pb-10">
                {storageError && (
                  <div className="mb-4 p-3 rounded-xl border border-yellow-500/50 bg-yellow-50 dark:bg-yellow-950/20">
                    <p className="text-yellow-800 dark:text-yellow-200 text-xs">
                      {t("saved.storageUnavailable")}
                    </p>
                  </div>
                )}
                <SavedItemsList />
              </div>
            </div>
          </div>
        </>
      )}
    </>
  );
}

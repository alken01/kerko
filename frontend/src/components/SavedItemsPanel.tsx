"use client";

import { useState, useEffect } from "react";
import { Bookmark, X, Smartphone } from "lucide-react";
import { useSavedItems } from "@/contexts/SavedItemsContext";
import { useTranslation } from "@/i18n/TranslationContext";
import { SavedItemsList } from "./SavedItemsList";
import { PWAInstallGuide } from "./PWAInstallGuide";
import { cn } from "@/lib/utils";

interface SavedItemsPanelProps {
  onNameClick: (emri: string, mbiemri: string) => void;
}

export function SavedItemsPanel({ onNameClick }: SavedItemsPanelProps) {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [showPWABanner, setShowPWABanner] = useState(false);
  const [isStandalone, setIsStandalone] = useState(false);
  const { savedCount } = useSavedItems();

  useEffect(() => {
    // Check if already running as PWA
    const standalone =
      window.matchMedia("(display-mode: standalone)").matches ||
      (window.navigator as Navigator & { standalone?: boolean }).standalone === true;
    setIsStandalone(standalone);

    // Only show PWA banner if not in standalone mode and not dismissed
    const dismissed = localStorage.getItem("pwa-banner-dismissed");
    setShowPWABanner(!standalone && dismissed !== "true");
  }, []);

  const dismissPWABanner = () => {
    localStorage.setItem("pwa-banner-dismissed", "true");
    setShowPWABanner(false);
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

      {/* Full Screen Panel */}
      {isOpen && (
        <div className="fixed inset-0 z-50 bg-surface-primary overflow-hidden flex flex-col">
          {/* Header */}
          <div className="flex-shrink-0 bg-surface-primary border-b border-border-semantic-secondary">
            <div className="container mx-auto px-4 md:px-6 py-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Bookmark className="h-6 w-6 text-accent-saved fill-current" />
                  <div>
                    <h2 className="text-lg font-semibold text-text-primary">
                      {t("saved.savedItems")}
                    </h2>
                    {savedCount > 0 && (
                      <p className="text-xs text-text-secondary">
                        {savedCount} {savedCount === 1 ? t("saved.result") : t("saved.results")}
                      </p>
                    )}
                  </div>
                </div>
                <button
                  onClick={() => setIsOpen(false)}
                  className="w-10 h-10 rounded-xl flex items-center justify-center hover:bg-surface-interactive transition-colors"
                >
                  <X className="h-5 w-5 text-text-secondary" />
                </button>
              </div>
            </div>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-auto">
            <div className="container mx-auto px-4 md:px-6 py-6 pb-32">
            {/* PWA Install Guide Banner - Dismissable, hidden when already in PWA */}
            {showPWABanner && !isStandalone && (
              <div className="mb-6 p-4 rounded-xl bg-surface-secondary border-2 border-border-semantic-secondary relative">
                <button
                  onClick={dismissPWABanner}
                  className="absolute top-3 right-3 p-1 rounded-md hover:bg-surface-interactive transition-colors"
                >
                  <X className="h-4 w-4 text-text-tertiary" />
                </button>
                <div className="flex items-start gap-3 pr-6">
                  <div className="flex-shrink-0 w-9 h-9 rounded-full bg-accent-saved/20 flex items-center justify-center">
                    <Smartphone className="h-4 w-4 text-accent-saved" />
                  </div>
                  <div className="flex-1">
                    <h3 className="font-medium text-text-primary text-sm mb-1">
                      {t("saved.accessOffline")}
                    </h3>
                    <p className="text-xs text-text-secondary mb-3">
                      {t("saved.installForOffline")}
                    </p>
                    <PWAInstallGuide />
                  </div>
                </div>
              </div>
            )}

            {/* Saved Items List */}
            <SavedItemsList onNameClick={(emri, mbiemri) => {
              onNameClick(emri, mbiemri);
              setIsOpen(false);
            }} />
            </div>
          </div>
        </div>
      )}
    </>
  );
}

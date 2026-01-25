"use client";

import { useState, useEffect } from "react";
import { Bookmark, X, Smartphone } from "lucide-react";
import { useSavedItems } from "@/contexts/SavedItemsContext";
import { SavedItemsList } from "./SavedItemsList";
import { PWAInstallGuide } from "./PWAInstallGuide";
import { cn } from "@/lib/utils";

interface SavedItemsPanelProps {
  onNameClick: (emri: string, mbiemri: string) => void;
}

export function SavedItemsPanel({ onNameClick }: SavedItemsPanelProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [showPWABanner, setShowPWABanner] = useState(false);
  const { savedCount } = useSavedItems();

  useEffect(() => {
    const dismissed = localStorage.getItem("pwa-banner-dismissed");
    setShowPWABanner(dismissed !== "true");
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
          "flex items-center gap-1.5 px-3 py-2 rounded-full",
          "bg-surface-tertiary text-text-primary",
          "border-2 border-border-semantic-primary",
          "shadow-lg hover:shadow-xl",
          "transition-all duration-200 hover:scale-105",
          "text-xs font-medium"
        )}
      >
        <Bookmark className="h-4 w-4" />
        <span>Të Ruajturat</span>
        {savedCount > 0 && (
          <span className="bg-amber-500 text-amber-950 text-[10px] font-bold px-1.5 py-0.5 rounded-full">
            {savedCount}
          </span>
        )}
      </button>

      {/* Full Screen Panel */}
      {isOpen && (
        <div className="fixed inset-0 z-50 bg-surface-primary">
          {/* Header */}
          <div className="sticky top-0 z-10 bg-surface-primary border-b-2 border-border-semantic-secondary">
            <div className="container mx-auto px-4 md:px-6 py-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Bookmark className="h-6 w-6 text-amber-500" />
                  <h2 className="text-xl font-bold text-text-primary">
                    Të Ruajturat
                  </h2>
                  {savedCount > 0 && (
                    <span className="bg-amber-500 text-amber-950 text-xs font-bold px-2 py-0.5 rounded-full">
                      {savedCount}
                    </span>
                  )}
                </div>
                <button
                  onClick={() => setIsOpen(false)}
                  className="p-2 rounded-lg hover:bg-surface-interactive transition-colors"
                >
                  <X className="h-6 w-6 text-text-tertiary" />
                </button>
              </div>
            </div>
          </div>

          {/* Content */}
          <div className="container mx-auto px-4 md:px-6 py-6 pb-32 overflow-auto max-h-[calc(100vh-80px)]">
            {/* PWA Install Guide Banner - Dismissable */}
            {showPWABanner && (
              <div className="mb-6 p-4 rounded-xl bg-surface-secondary border-2 border-border-semantic-secondary relative">
                <button
                  onClick={dismissPWABanner}
                  className="absolute top-3 right-3 p-1 rounded-md hover:bg-surface-interactive transition-colors"
                >
                  <X className="h-4 w-4 text-text-tertiary" />
                </button>
                <div className="flex items-start gap-3 pr-6">
                  <div className="flex-shrink-0 w-9 h-9 rounded-full bg-amber-500/20 flex items-center justify-center">
                    <Smartphone className="h-4 w-4 text-amber-500" />
                  </div>
                  <div className="flex-1">
                    <h3 className="font-medium text-text-primary text-sm mb-1">
                      Akseso offline
                    </h3>
                    <p className="text-xs text-text-secondary mb-3">
                      Instalo si aplikacion për të parë të ruajturat pa internet.
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
      )}
    </>
  );
}

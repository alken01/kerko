"use client";

import { useState, useEffect } from "react";
import { Download, X, Smartphone, Share, Plus } from "lucide-react";
import { cn } from "@/lib/utils";

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: "accepted" | "dismissed" }>;
}

export function PWAInstallGuide() {
  const [showGuide, setShowGuide] = useState(false);
  const [deferredPrompt, setDeferredPrompt] =
    useState<BeforeInstallPromptEvent | null>(null);
  const [isIOS, setIsIOS] = useState(false);
  const [isStandalone, setIsStandalone] = useState(false);

  useEffect(() => {
    const standalone =
      window.matchMedia("(display-mode: standalone)").matches ||
      (window.navigator as Navigator & { standalone?: boolean }).standalone === true;
    setIsStandalone(standalone);

    const iOS = /iPad|iPhone|iPod/.test(navigator.userAgent);
    setIsIOS(iOS);

    const handleBeforeInstall = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e as BeforeInstallPromptEvent);
    };

    window.addEventListener("beforeinstallprompt", handleBeforeInstall);

    return () => {
      window.removeEventListener("beforeinstallprompt", handleBeforeInstall);
    };
  }, []);

  const handleInstallClick = async () => {
    if (deferredPrompt) {
      deferredPrompt.prompt();
      const { outcome } = await deferredPrompt.userChoice;
      if (outcome === "accepted") {
        setDeferredPrompt(null);
        setShowGuide(false);
      }
    } else {
      setShowGuide(true);
    }
  };

  if (isStandalone) {
    return null;
  }

  return (
    <>
      <button
        onClick={handleInstallClick}
        className={cn(
          "flex items-center gap-2 px-3 py-1.5 rounded-lg text-xs font-medium",
          "bg-surface-interactive border border-border-semantic-secondary",
          "text-text-secondary hover:text-text-primary",
          "transition-all duration-200"
        )}
      >
        <Download className="h-3.5 w-3.5" />
        <span>Instalo</span>
      </button>

      {showGuide && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/50">
          <div className="bg-surface-primary border-2 border-border-semantic-secondary rounded-xl max-w-sm w-full p-4 shadow-xl animate-in fade-in zoom-in-95 duration-200">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-base font-semibold text-text-primary flex items-center gap-2">
                <Smartphone className="h-4 w-4" />
                Instalo aplikacionin
              </h3>
              <button
                onClick={() => setShowGuide(false)}
                className="p-1 rounded-md hover:bg-surface-interactive transition-colors"
              >
                <X className="h-4 w-4 text-text-tertiary" />
              </button>
            </div>

            <div className="text-text-secondary text-sm">
              {isIOS ? (
                <ol className="list-none space-y-2">
                  <li className="flex items-center gap-2">
                    <span className="flex-shrink-0 w-5 h-5 rounded-full bg-surface-interactive flex items-center justify-center text-xs font-medium">
                      1
                    </span>
                    <span>
                      Shtyp <Share className="inline h-3.5 w-3.5 text-blue-500" /> në Safari
                    </span>
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="flex-shrink-0 w-5 h-5 rounded-full bg-surface-interactive flex items-center justify-center text-xs font-medium">
                      2
                    </span>
                    <span>
                      Zgjidh <Plus className="inline h-3.5 w-3.5" /> &quot;Add to Home Screen&quot;
                    </span>
                  </li>
                </ol>
              ) : (
                <ol className="list-none space-y-2">
                  <li className="flex items-center gap-2">
                    <span className="flex-shrink-0 w-5 h-5 rounded-full bg-surface-interactive flex items-center justify-center text-xs font-medium">
                      1
                    </span>
                    <span>Shtyp menunë (⋮) në Chrome</span>
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="flex-shrink-0 w-5 h-5 rounded-full bg-surface-interactive flex items-center justify-center text-xs font-medium">
                      2
                    </span>
                    <span>Zgjidh &quot;Install app&quot;</span>
                  </li>
                </ol>
              )}
            </div>

            <button
              onClick={() => setShowGuide(false)}
              className={cn(
                "w-full mt-4 py-2 px-4 rounded-lg text-sm font-medium",
                "bg-surface-tertiary text-text-primary",
                "hover:bg-surface-interactive transition-colors"
              )}
            >
              E kuptova
            </button>
          </div>
        </div>
      )}
    </>
  );
}

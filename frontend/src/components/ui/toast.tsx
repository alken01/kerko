"use client";

import { createContext, useContext, useState, useCallback, ReactNode } from "react";
import { Bookmark, X, Smartphone } from "lucide-react";
import { useTranslation } from "@/i18n/TranslationContext";
import { cn } from "@/lib/utils";

interface Toast {
  id: string;
  message: string;
  type: "saved" | "removed" | "info";
  showPWAHint?: boolean;
}

interface ToastContextType {
  showToast: (message: string, type: Toast["type"], showPWAHint?: boolean) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export function ToastProvider({ children }: { children: ReactNode }) {
  const { t } = useTranslation();
  const [toasts, setToasts] = useState<Toast[]>([]);

  const showToast = useCallback(
    (message: string, type: Toast["type"], showPWAHint?: boolean) => {
      const id = crypto.randomUUID();
      setToasts((prev) => [...prev, { id, message, type, showPWAHint }]);

      setTimeout(() => {
        setToasts((prev) => prev.filter((t) => t.id !== id));
      }, showPWAHint ? 5000 : 3000);
    },
    []
  );

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      <div className="fixed bottom-24 left-4 right-4 z-50 flex flex-col gap-2 pointer-events-none">
        {toasts.map((toast) => (
          <div
            key={toast.id}
            className={cn(
              "pointer-events-auto mx-auto max-w-sm w-full",
              "bg-surface-secondary border-2 border-border-semantic-secondary",
              "rounded-xl p-4 shadow-lg",
              "animate-in slide-in-from-bottom-4 fade-in duration-300"
            )}
          >
            <div className="flex items-start gap-3">
              <div
                className={cn(
                  "flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center",
                  toast.type === "saved" && "bg-amber-500/20",
                  toast.type === "removed" && "bg-surface-interactive",
                  toast.type === "info" && "bg-blue-500/20"
                )}
              >
                <Bookmark
                  className={cn(
                    "h-4 w-4",
                    toast.type === "saved" && "text-amber-500 fill-amber-500",
                    toast.type === "removed" && "text-text-tertiary",
                    toast.type === "info" && "text-blue-500"
                  )}
                />
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-text-primary">
                  {toast.message}
                </p>
                {toast.showPWAHint && (
                  <p className="text-xs text-text-secondary mt-1 flex items-center gap-1">
                    <Smartphone className="h-3 w-3" />
                    {t("pwa.installForOfflineAccess")}
                  </p>
                )}
              </div>
              <button
                onClick={() => removeToast(toast.id)}
                className="flex-shrink-0 p-1 rounded hover:bg-surface-interactive transition-colors"
              >
                <X className="h-4 w-4 text-text-tertiary" />
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (context === undefined) {
    throw new Error("useToast must be used within a ToastProvider");
  }
  return context;
}

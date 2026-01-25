"use client";

import { useEffect } from "react";

export function GlobalStyles() {
  useEffect(() => {
    // iOS keyboard handling - scroll focused input into view
    const handleFocus = (e: FocusEvent) => {
      const target = e.target as HTMLElement;
      if (
        target.tagName === "INPUT" ||
        target.tagName === "TEXTAREA" ||
        target.tagName === "SELECT"
      ) {
        // Small delay to let keyboard appear
        setTimeout(() => {
          target.scrollIntoView({ behavior: "smooth", block: "center" });
        }, 300);
      }
    };

    // Visual viewport resize handler for iOS keyboard
    const handleViewportResize = () => {
      if (window.visualViewport) {
        const viewport = window.visualViewport;
        // Adjust body height when keyboard is shown
        document.documentElement.style.setProperty(
          "--viewport-height",
          `${viewport.height}px`
        );
      }
    };

    document.addEventListener("focusin", handleFocus);

    if (window.visualViewport) {
      window.visualViewport.addEventListener("resize", handleViewportResize);
      // Initial call
      handleViewportResize();
    }

    return () => {
      document.removeEventListener("focusin", handleFocus);
      if (window.visualViewport) {
        window.visualViewport.removeEventListener(
          "resize",
          handleViewportResize
        );
      }
    };
  }, []);

  return (
    <style jsx global>{`
      body {
        overscroll-behavior: none;
        touch-action: manipulation;
        -webkit-tap-highlight-color: transparent;
      }
      @supports (-webkit-touch-callout: none) {
        .min-h-screen {
          min-height: -webkit-fill-available;
        }
      }
      ::-webkit-scrollbar {
        display: none;
      }
      * {
        -ms-overflow-style: none;
        scrollbar-width: none;
      }
    `}</style>
  );
}

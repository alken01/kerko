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
        // Use requestAnimationFrame for smoother scrolling that doesn't fight momentum
        requestAnimationFrame(() => {
          target.scrollIntoView({ block: "center" });
        });
      }
    };

    // Visual viewport resize handler for iOS keyboard
    const handleViewportResize = () => {
      if (window.visualViewport) {
        document.documentElement.style.setProperty(
          "--viewport-height",
          `${window.visualViewport.height}px`
        );
      }
    };

    document.addEventListener("focusin", handleFocus);

    if (window.visualViewport) {
      window.visualViewport.addEventListener("resize", handleViewportResize);
      handleViewportResize();
    }

    return () => {
      document.removeEventListener("focusin", handleFocus);
      if (window.visualViewport) {
        window.visualViewport.removeEventListener("resize", handleViewportResize);
      }
    };
  }, []);

  return null;
}

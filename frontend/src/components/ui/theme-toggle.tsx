"use client";

import { useTheme } from "@/contexts/ThemeContext";
import { Button } from "@/components/ui/button";
import { Moon, Sun } from "lucide-react";

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={toggleTheme}
      className="h-9 w-9 bg-surface-interactive hover:bg-surface-interactive-hover border-2 border-border-semantic-secondary transition-colors"
    >
      {theme === "light" ? (
        <Moon className="h-4 w-4 text-text-primary" />
      ) : (
        <Sun className="h-4 w-4 text-text-primary" />
      )}
      <span className="sr-only">Toggle theme</span>
    </Button>
  );
}
"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Shield } from "lucide-react";
import Link from "next/link";
import { TOKEN_KEY } from "@/app/admin/api";

export function AdminLink() {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    setVisible(!!localStorage.getItem(TOKEN_KEY));
  }, []);

  if (!visible) return null;

  return (
    <Button
      variant="ghost"
      size="icon"
      asChild
      className="h-9 w-9 bg-surface-interactive hover:bg-surface-interactive-hover border-2 border-border-semantic-secondary transition-colors"
    >
      <Link href="/admin">
        <Shield className="h-4 w-4 text-text-primary" />
        <span className="sr-only">Admin dashboard</span>
      </Link>
    </Button>
  );
}

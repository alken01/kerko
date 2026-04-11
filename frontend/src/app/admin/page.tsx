"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { TokenGate } from "./TokenGate";
import { StatsCards } from "./StatsCards";
import { Filters } from "./Filters";
import { LogsTable } from "./LogsTable";
import { LogFilters, TOKEN_KEY, VersionResponse, fetchVersion } from "./api";
import Link from "next/link";

export default function AdminPage() {
  const [token, setToken] = useState<string | null>(null);
  const [mounted, setMounted] = useState(false);
  const [filters, setFilters] = useState<LogFilters>({});
  const [version, setVersion] = useState<VersionResponse | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem(TOKEN_KEY);
    setToken(stored);
    setMounted(true);
    fetchVersion().then(setVersion).catch(() => {});
  }, []);

  function handleAuthenticated() {
    const stored = localStorage.getItem(TOKEN_KEY);
    setToken(stored);
  }

  function handleSignOut() {
    localStorage.removeItem(TOKEN_KEY);
    window.location.reload();
  }

  function handleUnauthorized() {
    localStorage.removeItem(TOKEN_KEY);
    setToken(null);
  }

  // Avoid flash of content before localStorage is read
  if (!mounted) {
    return null;
  }

  if (!token) {
    return <TokenGate onAuthenticated={handleAuthenticated} />;
  }

  return (
    <div className="min-h-screen bg-surface-primary overflow-x-hidden touch-manipulation">
      <main className="container mx-auto py-4 space-y-3 px-4 md:px-6 max-w-5xl">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Link
              href="/"
              className="text-text-tertiary hover:text-text-primary transition-colors text-sm"
            >
              &larr;
            </Link>
            <h1 className="text-2xl font-bold tracking-tight text-text-primary">
              Admin
            </h1>
            {version && (
              <span className="text-[11px] text-text-tertiary font-mono">
                {version.sha}{version.deployed !== "unknown" && ` · ${new Date(version.deployed).toLocaleDateString()}`}
              </span>
            )}
          </div>
          <Button
            variant="outline"
            onClick={handleSignOut}
            className="bg-surface-secondary border-2 border-border-semantic-secondary text-text-tertiary hover:bg-surface-tertiary hover:text-text-primary text-sm h-8 touch-manipulation"
          >
            Sign out
          </Button>
        </div>

        {/* Stats */}
        <section>
          <StatsCards onUnauthorized={handleUnauthorized} />
        </section>

        {/* Filters */}
        <section>
          <Filters onApply={setFilters} />
        </section>

        {/* Logs */}
        <section className="space-y-2">
          <h2 className="text-sm font-bold">
            Request logs
          </h2>
          <LogsTable filters={filters} onUnauthorized={handleUnauthorized} />
        </section>
      </main>
    </div>
  );
}

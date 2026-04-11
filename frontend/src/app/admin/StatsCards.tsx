"use client";

import { useState, useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { cardStyles } from "@/components/ui/card-styles";
import {
  fetchStats,
  StatsWindow,
  StatsResponse,
  AdminUnauthorizedError,
} from "./api";

interface StatsCardsProps {
  onUnauthorized: () => void;
}

const WINDOWS: StatsWindow[] = ["1h", "24h", "7d", "30d"];

function StatBadge({
  label,
  value,
}: {
  label: string;
  value: string | number;
}) {
  return (
    <span className="text-text-tertiary text-sm">
      {label}: <span className="text-text-primary font-bold">{value}</span>
    </span>
  );
}

export function StatsCards({ onUnauthorized }: StatsCardsProps) {
  const [window, setWindow] = useState<StatsWindow>("24h");
  const [stats, setStats] = useState<StatsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    fetchStats(window)
      .then((data) => {
        if (!cancelled) {
          setStats(data);
          setLoading(false);
        }
      })
      .catch((err) => {
        if (cancelled) return;
        if (err instanceof AdminUnauthorizedError) {
          onUnauthorized();
        } else {
          setError("Failed to load stats.");
        }
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [window, onUnauthorized]);

  return (
    <div className="space-y-3">
      {/* Window selector — tab style matching SearchForm */}
      <div className="flex space-x-1 p-0.5 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary">
        {WINDOWS.map((w) => (
          <button
            key={w}
            onClick={() => setWindow(w)}
            className={`flex-1 px-3 py-1.5 text-xs font-medium rounded-md transition-all duration-200 touch-manipulation ${
              window === w
                ? "bg-surface-tertiary text-text-primary"
                : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
            }`}
          >
            {w}
          </button>
        ))}
      </div>

      {loading && (
        <p className="text-text-tertiary text-sm">Loading stats...</p>
      )}

      {error && (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {stats && !loading && (
        <Card className={cardStyles.root}>
          <CardContent className="p-4 space-y-3">
            {/* Counters — single line */}
            <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm">
              <StatBadge label={`Total (${window})`} value={stats.total.toLocaleString()} />
              <StatBadge label="Today" value={stats.totalToday.toLocaleString()} />
              <StatBadge label="7d" value={stats.totalLast7d.toLocaleString()} />
            </div>

            {/* Top IPs + Top Queries side by side on desktop, stacked on mobile */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div className="space-y-1">
                <p className="text-text-tertiary text-xs font-medium">Top IPs</p>
                {stats.topIps.length === 0 ? (
                  <p className="text-text-tertiary text-xs">No data.</p>
                ) : (
                  <ol className="space-y-0.5">
                    {stats.topIps.slice(0, 5).map((item, i) => (
                      <li key={item.ip} className="flex justify-between text-xs text-text-primary">
                        <span>
                          <span className="text-text-tertiary mr-1.5">{i + 1}.</span>
                          <span className="font-mono">{item.ip}</span>
                        </span>
                        <span className="font-bold">{item.count}</span>
                      </li>
                    ))}
                  </ol>
                )}
              </div>

              <div className="space-y-1">
                <p className="text-text-tertiary text-xs font-medium">Top Queries</p>
                {stats.topQueries.length === 0 ? (
                  <p className="text-text-tertiary text-xs">No data.</p>
                ) : (
                  <ol className="space-y-0.5">
                    {stats.topQueries.slice(0, 5).map((item, i) => (
                      <li key={item.term} className="flex justify-between text-xs text-text-primary gap-2">
                        <span className="truncate">
                          <span className="text-text-tertiary mr-1.5">{i + 1}.</span>
                          {item.term}
                        </span>
                        <span className="font-bold shrink-0">{item.count}</span>
                      </li>
                    ))}
                  </ol>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

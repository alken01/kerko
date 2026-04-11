"use client";

import { useState, useEffect, useCallback } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { cardStyles } from "@/components/ui/card-styles";
import {
  fetchLogs,
  LogFilters,
  RequestLog,
  AdminUnauthorizedError,
} from "./api";

interface LogsTableProps {
  filters: LogFilters;
  onUnauthorized: () => void;
}

function formatQuerySummary(log: RequestLog): string {
  const parts: string[] = [];
  if (log.emri) parts.push(`emri=${log.emri}`);
  if (log.mbiemri) parts.push(`mbiemri=${log.mbiemri}`);
  if (log.numriTarges) parts.push(`targat=${log.numriTarges}`);
  if (log.numriTelefonit) parts.push(`telefon=${log.numriTelefonit}`);
  return parts.join(" ") || "—";
}

function formatLocalTime(utcString: string): string {
  return new Date(utcString).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

function formatUtcTime(utcString: string): string {
  return new Date(utcString).toISOString();
}

function statusColor(code: number): string {
  if (code >= 500) return "text-destructive";
  if (code >= 400) return "text-yellow-600 dark:text-yellow-400";
  return "text-green-700 dark:text-green-400";
}

function LogCard({ log }: { log: RequestLog }) {
  const localTime = formatLocalTime(log.timestampUtc);
  const utcTime = formatUtcTime(log.timestampUtc);

  return (
    <Card className={cardStyles.root}>
      <CardContent className="p-3 space-y-1.5">
        {/* Time */}
        <div>
          <p className="text-text-primary text-xs font-medium">{localTime}</p>
          <p className="text-text-tertiary text-[11px]">{utcTime}</p>
        </div>

        {/* Endpoint + query */}
        <div className="flex flex-wrap gap-x-3 gap-y-1 text-sm">
          <span className="font-mono text-text-tertiary">{log.endpoint}</span>
          <span className="text-text-primary truncate">{formatQuerySummary(log)}</span>
        </div>

        {/* IP + UA */}
        <div className="text-xs text-text-tertiary">
          <span className="font-mono">{log.clientIp}</span>
          {" · "}
          <span>{log.userAgentSimplified || log.userAgentRaw}</span>
        </div>

        {/* Status + duration + result count */}
        <div className="flex gap-3 text-xs">
          <span className={`font-bold ${statusColor(log.statusCode)}`}>
            {log.statusCode}
          </span>
          <span className="text-text-tertiary">{log.durationMs}ms</span>
          {log.resultCount !== null && (
            <span className="text-text-tertiary">{log.resultCount} results</span>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

export function LogsTable({ filters, onUnauthorized }: LogsTableProps) {
  const [logs, setLogs] = useState<RequestLog[]>([]);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadInitial = useCallback(() => {
    setLoading(true);
    setError(null);
    setLogs([]);
    setNextCursor(null);

    fetchLogs({ ...filters, limit: 50 })
      .then((data) => {
        setLogs(data.items);
        setNextCursor(data.nextCursor);
        setLoading(false);
      })
      .catch((err) => {
        if (err instanceof AdminUnauthorizedError) {
          onUnauthorized();
        } else {
          setError("Failed to load logs.");
        }
        setLoading(false);
      });
  }, [filters, onUnauthorized]);

  useEffect(() => {
    loadInitial();
  }, [loadInitial]);

  function loadMore() {
    if (!nextCursor || loadingMore) return;
    setLoadingMore(true);

    fetchLogs({ ...filters, limit: 50, cursor: nextCursor })
      .then((data) => {
        setLogs((prev) => [...prev, ...data.items]);
        setNextCursor(data.nextCursor);
        setLoadingMore(false);
      })
      .catch((err) => {
        if (err instanceof AdminUnauthorizedError) {
          onUnauthorized();
        } else {
          setError("Failed to load more logs.");
        }
        setLoadingMore(false);
      });
  }

  if (loading) {
    return <p className="text-text-tertiary text-sm">Loading logs...</p>;
  }

  if (error) {
    return (
      <Alert variant="destructive">
        <AlertDescription>{error}</AlertDescription>
      </Alert>
    );
  }

  if (logs.length === 0) {
    return (
      <div className="text-center py-8 space-y-2">
        <div className="text-4xl">:/</div>
        <p className="text-text-tertiary text-sm">No logs found.</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {/* Mobile: cards */}
      <div className="md:hidden space-y-2">
        {logs.map((log) => (
          <LogCard key={log.id} log={log} />
        ))}
      </div>

      {/* Desktop: table */}
      <div className="hidden md:block">
        <Card className={cardStyles.root}>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-surface-secondary border-b-2 border-border-semantic-secondary">
                <tr>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">Time</th>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">Endpoint</th>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">Query</th>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">IP / UA</th>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">Status</th>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">Duration</th>
                  <th className="text-left px-3 py-2 text-text-tertiary font-medium text-xs">Results</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border-semantic-secondary text-text-primary">
                {logs.map((log) => {
                  const localTime = formatLocalTime(log.timestampUtc);
                  const utcTime = formatUtcTime(log.timestampUtc);
                  return (
                    <tr key={log.id} className="hover:bg-surface-secondary transition-colors">
                      <td className="px-3 py-2 whitespace-nowrap">
                        <span title={utcTime}>{localTime}</span>
                      </td>
                      <td className="px-3 py-2 font-mono">{log.endpoint}</td>
                      <td className="px-3 py-2 max-w-xs truncate">{formatQuerySummary(log)}</td>
                      <td className="px-3 py-2 text-xs text-text-tertiary whitespace-nowrap">
                        <div className="font-mono">{log.clientIp}</div>
                        <div>{log.userAgentSimplified || log.userAgentRaw}</div>
                      </td>
                      <td className={`px-3 py-2 font-bold ${statusColor(log.statusCode)}`}>
                        {log.statusCode}
                      </td>
                      <td className="px-3 py-2 text-text-tertiary">{log.durationMs}ms</td>
                      <td className="px-3 py-2 text-text-tertiary">
                        {log.resultCount ?? "—"}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </Card>
      </div>

      {nextCursor && (
        <div className="flex justify-center pt-2">
          <Button
            onClick={loadMore}
            disabled={loadingMore}
            variant="outline"
            className="bg-surface-secondary border-2 border-border-semantic-secondary hover:bg-surface-tertiary h-9 px-8 text-sm touch-manipulation"
          >
            {loadingMore ? "Loading..." : "Load more"}
          </Button>
        </div>
      )}
    </div>
  );
}

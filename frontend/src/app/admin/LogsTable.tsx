"use client";

import { useState, useEffect, useCallback, useMemo } from "react";
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
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

function formatUtcTime(utcString: string): string {
  return new Date(utcString).toISOString();
}

function formatDateHeader(utcString: string): string {
  const date = new Date(utcString);
  const now = new Date();
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const logDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const diffDays = Math.round(
    (today.getTime() - logDate.getTime()) / (1000 * 60 * 60 * 24)
  );

  if (diffDays === 0) return "Today";
  if (diffDays === 1) return "Yesterday";

  return date.toLocaleDateString(undefined, {
    weekday: "short",
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function getDateKey(utcString: string): string {
  const d = new Date(utcString);
  return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

function statusColor(code: number): string {
  if (code >= 500) return "text-destructive";
  if (code >= 400) return "text-yellow-600 dark:text-yellow-400";
  return "text-green-700 dark:text-green-400";
}

const thClass = "text-left px-3 py-2 text-text-tertiary font-medium text-xs";

function DateSeparator({ label }: { label: string }) {
  return (
    <div className="flex items-center gap-3 py-2">
      <div className="h-px flex-1 bg-border-semantic-secondary" />
      <span className="text-xs font-medium text-text-tertiary shrink-0">
        {label}
      </span>
      <div className="h-px flex-1 bg-border-semantic-secondary" />
    </div>
  );
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

        {/* IP + Location + UA */}
        <div className="text-xs text-text-tertiary">
          <span className="font-mono">{log.clientIp}</span>
          {log.location && (
            <span className="text-text-secondary"> · {log.location}</span>
          )}
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

interface GroupedLogs {
  dateKey: string;
  dateLabel: string;
  logs: RequestLog[];
}

function groupLogsByDate(logs: RequestLog[]): GroupedLogs[] {
  const groups: GroupedLogs[] = [];
  let currentKey = "";

  for (const log of logs) {
    const key = getDateKey(log.timestampUtc);
    if (key !== currentKey) {
      currentKey = key;
      groups.push({
        dateKey: key,
        dateLabel: formatDateHeader(log.timestampUtc),
        logs: [],
      });
    }
    groups[groups.length - 1].logs.push(log);
  }

  return groups;
}

export function LogsTable({ filters, onUnauthorized }: LogsTableProps) {
  const [logs, setLogs] = useState<RequestLog[]>([]);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const groups = useMemo(() => groupLogsByDate(logs), [logs]);

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
      {/* Mobile: cards grouped by date */}
      <div className="md:hidden space-y-2">
        {groups.map((group) => (
          <div key={group.dateKey}>
            <DateSeparator label={group.dateLabel} />
            <div className="space-y-2">
              {group.logs.map((log) => (
                <LogCard key={log.id} log={log} />
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* Desktop: table grouped by date */}
      <div className="hidden md:block space-y-3">
        {groups.map((group) => (
          <div key={group.dateKey}>
            <DateSeparator label={group.dateLabel} />
            <Card className={cardStyles.root}>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="bg-surface-secondary border-b-2 border-border-semantic-secondary">
                    <tr>
                      <th className={thClass}>Time</th>
                      <th className={thClass}>Endpoint</th>
                      <th className={thClass}>Query</th>
                      <th className={thClass}>IP / Location</th>
                      <th className={thClass}>UA</th>
                      <th className={thClass}>Status</th>
                      <th className={thClass}>Duration</th>
                      <th className={thClass}>Results</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border-semantic-secondary text-text-primary">
                    {group.logs.map((log) => {
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
                            {log.location && (
                              <div className="text-text-secondary">{log.location}</div>
                            )}
                          </td>
                          <td className="px-3 py-2 text-xs text-text-tertiary">
                            {log.userAgentSimplified || log.userAgentRaw}
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
        ))}
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

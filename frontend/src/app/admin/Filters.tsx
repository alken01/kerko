"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { cardStyles } from "@/components/ui/card-styles";
import { LogFilters } from "./api";

interface FiltersProps {
  onApply: (filters: LogFilters) => void;
}

export function Filters({ onApply }: FiltersProps) {
  const [open, setOpen] = useState(false);
  const [endpoint, setEndpoint] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [ip, setIp] = useState("");
  const [q, setQ] = useState("");
  const [status, setStatus] = useState("");

  function handleApply() {
    const filters: LogFilters = {};
    if (endpoint) filters.endpoint = endpoint;
    if (from) filters.from = new Date(from).toISOString();
    if (to) filters.to = new Date(to).toISOString();
    if (ip) filters.ip = ip;
    if (q) filters.q = q;
    if (status) filters.status = status;
    onApply(filters);
    setOpen(false);
  }

  function handleReset() {
    setEndpoint("");
    setFrom("");
    setTo("");
    setIp("");
    setQ("");
    setStatus("");
    onApply({});
    setOpen(false);
  }

  const inputClassName =
    "bg-surface-secondary border-2 border-border-semantic-secondary placeholder:text-text-tertiary focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-9 text-sm touch-manipulation";

  return (
    <Card className={`${cardStyles.root} gap-0`}>
      <button
        onClick={() => setOpen((o) => !o)}
        className="w-full flex items-center justify-between px-4 py-3 text-sm font-medium text-text-primary touch-manipulation"
      >
        <span>Filters</span>
        <span className="text-text-tertiary text-xs">{open ? "▲" : "▼"}</span>
      </button>

      {open && (
        <div className="mx-4 mb-4 pt-4 px-0 space-y-3 border-t-2 border-border-semantic-secondary">
          {/* Endpoint + Status code */}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className="text-xs text-text-tertiary">Endpoint</label>
              <select
                value={endpoint}
                onChange={(e) => setEndpoint(e.target.value)}
                className="w-full rounded-md bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary text-sm px-3 h-9 touch-manipulation focus:outline-none focus:ring-1 focus:ring-border-semantic-interactive"
              >
                <option value="">All</option>
                <option value="kerko">kerko</option>
                <option value="targat">targat</option>
                <option value="telefon">telefon</option>
              </select>
            </div>
            <div className="space-y-1">
              <label className="text-xs text-text-tertiary">Status code</label>
              <Input
                type="number"
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                placeholder="e.g. 200"
                className={inputClassName}
              />
            </div>
          </div>

          {/* Date range */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className="text-xs text-text-tertiary">From</label>
              <Input
                type="datetime-local"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
                className={inputClassName}
              />
            </div>
            <div className="space-y-1">
              <label className="text-xs text-text-tertiary">To</label>
              <Input
                type="datetime-local"
                value={to}
                onChange={(e) => setTo(e.target.value)}
                className={inputClassName}
              />
            </div>
          </div>

          {/* IP + Query side by side */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div className="space-y-1">
              <label className="text-xs text-text-tertiary">IP contains</label>
              <Input
                type="text"
                value={ip}
                onChange={(e) => setIp(e.target.value)}
                placeholder="e.g. 192.168"
                className={inputClassName}
              />
            </div>
            <div className="space-y-1">
              <label className="text-xs text-text-tertiary">Query contains</label>
              <Input
                type="text"
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="e.g. Alban"
                className={inputClassName}
              />
            </div>
          </div>

          <div className="flex gap-3">
            <Button
              type="button"
              variant="outline"
              onClick={handleApply}
              className="flex-1 bg-surface-secondary border-2 border-border-semantic-secondary hover:bg-surface-tertiary h-9 text-sm touch-manipulation"
            >
              Apply
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={handleReset}
              className="flex-1 bg-surface-secondary border-2 border-border-semantic-secondary text-text-tertiary hover:bg-surface-tertiary hover:text-text-primary h-9 text-sm touch-manipulation"
            >
              Reset
            </Button>
          </div>
        </div>
      )}
    </Card>
  );
}

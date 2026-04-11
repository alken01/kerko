const API_URL = process.env.NEXT_PUBLIC_API_URL;
export const TOKEN_KEY = "kerko_admin_token";

export class AdminUnauthorizedError extends Error {
  constructor() {
    super("Unauthorized");
    this.name = "AdminUnauthorizedError";
  }
}

export class AdminApiError extends Error {
  constructor(
    public readonly status: number,
    message: string
  ) {
    super(message);
    this.name = "AdminApiError";
  }
}

function getToken(): string {
  if (typeof window === "undefined") return "";
  return localStorage.getItem(TOKEN_KEY) ?? "";
}

async function adminFetch<T>(
  path: string,
  token?: string
): Promise<T> {
  const t = token ?? getToken();
  const res = await fetch(`${API_URL}${path}`, {
    headers: {
      "X-Admin-Token": t,
    },
  });

  if (res.status === 401) {
    throw new AdminUnauthorizedError();
  }

  if (!res.ok) {
    throw new AdminApiError(res.status, `Request failed with status ${res.status}`);
  }

  return res.json() as Promise<T>;
}

export type StatsWindow = "1h" | "24h" | "7d" | "30d";

export interface IpCount {
  ip: string;
  count: number;
}

export interface QueryCount {
  term: string;
  count: number;
}

export interface StatsResponse {
  window: StatsWindow;
  total: number;
  totalToday: number;
  totalLast7d: number;
  topIps: IpCount[];
  topQueries: QueryCount[];
}

export interface RequestLog {
  id: number;
  timestampUtc: string;
  endpoint: string;
  emri: string | null;
  mbiemri: string | null;
  numriTarges: string | null;
  numriTelefonit: string | null;
  pageNumber: number;
  pageSize: number;
  clientIp: string;
  userAgentRaw: string;
  userAgentSimplified: string;
  statusCode: number;
  durationMs: number;
  resultCount: number | null;
  requestId: string;
}

export interface LogsResponse {
  items: RequestLog[];
  nextCursor: string | null;
}

export interface LogFilters {
  endpoint?: string;
  from?: string;
  to?: string;
  ip?: string;
  q?: string;
  status?: string;
  limit?: number;
  cursor?: string;
}

export interface VersionResponse {
  sha: string;
  deployed: string;
}

export async function fetchVersion(): Promise<VersionResponse> {
  const res = await fetch(`${API_URL}/api/version`);
  if (!res.ok) throw new Error("Failed to fetch version");
  return res.json() as Promise<VersionResponse>;
}

export async function fetchStats(
  window: StatsWindow,
  token?: string
): Promise<StatsResponse> {
  return adminFetch<StatsResponse>(`/api/admin/stats?window=${window}`, token);
}

export async function fetchLogs(
  filters: LogFilters,
  token?: string
): Promise<LogsResponse> {
  const params = new URLSearchParams();
  if (filters.endpoint) params.set("endpoint", filters.endpoint);
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  if (filters.ip) params.set("ip", filters.ip);
  if (filters.q) params.set("q", filters.q);
  if (filters.status) params.set("status", filters.status);
  if (filters.limit) params.set("limit", String(filters.limit));
  if (filters.cursor) params.set("cursor", filters.cursor);

  const qs = params.toString();
  return adminFetch<LogsResponse>(
    `/api/admin/logs${qs ? `?${qs}` : ""}`,
    token
  );
}

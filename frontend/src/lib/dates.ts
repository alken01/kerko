// Shared date formatting helpers. All API timestamps are UTC ISO strings
// (the backend serializes with a Z suffix).

/**
 * Reorders a slash-separated birth date from the source data's MM/DD/YY
 * layout to DD/MM/YY for display. Unexpected shapes are shown as-is rather
 * than hidden behind "N/A".
 */
export function formatBirthDate(dateString: string | null): string {
  if (!dateString) return "N/A";
  const parts = dateString.split("/");
  if (parts.length !== 3 || parts.some((part) => part === "")) {
    return dateString;
  }
  const [month, day, year] = parts;
  return `${day}/${month}/${year}`;
}

export function formatLocalTime(utcString: string): string {
  return new Date(utcString).toLocaleString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

export function formatUtcTime(utcString: string): string {
  return new Date(utcString).toISOString();
}

export function formatDateHeader(utcString: string): string {
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

export function getDateKey(utcString: string): string {
  const d = new Date(utcString);
  return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

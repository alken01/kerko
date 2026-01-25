import {
  PatronazhistResponse,
  PersonResponse,
  RrogatResponse,
  TargatResponse,
} from "./kerko";

export type SavedItemType = "person" | "rrogat" | "targat" | "patronazhist";

export interface SavedItem {
  id: string;
  type: SavedItemType;
  savedAt: number;
  displayName: string;
  key: string;
  data:
    | PersonResponse
    | RrogatResponse
    | TargatResponse
    | PatronazhistResponse;
}

export function generateItemKey(
  type: SavedItemType,
  data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
): string {
  switch (type) {
    case "person": {
      const p = data as PersonResponse;
      return `person:${p.emri}:${p.mbiemri}:${p.datelindja}:${p.qyteti}`;
    }
    case "rrogat": {
      const r = data as RrogatResponse;
      return `rrogat:${r.numriPersonal}:${r.nipt}`;
    }
    case "targat": {
      const t = data as TargatResponse;
      return `targat:${t.numriTarges}`;
    }
    case "patronazhist": {
      const pt = data as PatronazhistResponse;
      return `patronazhist:${pt.numriPersonal}:${pt.tel}`;
    }
  }
}

export function generateDisplayName(
  type: SavedItemType,
  data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
): string {
  switch (type) {
    case "person": {
      const p = data as PersonResponse;
      return `${p.emri || ""} ${p.mbiemri || ""}`.trim() || "Person";
    }
    case "rrogat": {
      const r = data as RrogatResponse;
      return `${r.emri || ""} ${r.mbiemri || ""}`.trim() || "Rrogat";
    }
    case "targat": {
      const t = data as TargatResponse;
      return t.numriTarges || "Targat";
    }
    case "patronazhist": {
      const pt = data as PatronazhistResponse;
      return `${pt.emri || ""} ${pt.mbiemri || ""}`.trim() || "Patronazhist";
    }
  }
}

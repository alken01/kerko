import { openDB, DBSchema, IDBPDatabase } from "idb";
import {
  SavedItem,
  SavedItemType,
  generateItemKey,
  generateDisplayName,
} from "@/types/saved";
import {
  PatronazhistResponse,
  PersonResponse,
  RrogatResponse,
  TargatResponse,
} from "@/types/kerko";

export interface SearchHistoryItem {
  id: string;
  type: "person" | "targa" | "telefon";
  terms: Record<string, string>;
  timestamp: number;
}

interface KerkoDBSchema extends DBSchema {
  savedItems: {
    key: string;
    value: SavedItem;
    indexes: {
      "by-type": SavedItemType;
      "by-savedAt": number;
      "by-key": string;
    };
  };
  searchHistory: {
    key: string;
    value: SearchHistoryItem;
    indexes: {
      "by-timestamp": number;
    };
  };
}

const DB_NAME = "kerko-db";
const DB_VERSION = 2;

let dbPromise: Promise<IDBPDatabase<KerkoDBSchema>> | null = null;

function getDB(): Promise<IDBPDatabase<KerkoDBSchema>> {
  if (!dbPromise) {
    dbPromise = openDB<KerkoDBSchema>(DB_NAME, DB_VERSION, {
      upgrade(db, oldVersion) {
        if (oldVersion < 1) {
          const store = db.createObjectStore("savedItems", { keyPath: "id" });
          store.createIndex("by-type", "type");
          store.createIndex("by-savedAt", "savedAt");
          store.createIndex("by-key", "key", { unique: true });
        }
        if (oldVersion < 2) {
          const historyStore = db.createObjectStore("searchHistory", {
            keyPath: "id",
          });
          historyStore.createIndex("by-timestamp", "timestamp");
        }
      },
    });
  }
  return dbPromise;
}

export async function saveItem(
  type: SavedItemType,
  data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
): Promise<SavedItem | null> {
  const db = await getDB();
  const key = generateItemKey(type, data);

  // Check for duplicate
  const existing = await db.getFromIndex("savedItems", "by-key", key);
  if (existing) {
    return null;
  }

  const item: SavedItem = {
    id: crypto.randomUUID(),
    type,
    savedAt: Date.now(),
    displayName: generateDisplayName(type, data),
    key,
    data,
  };

  await db.add("savedItems", item);
  return item;
}

export async function removeItem(id: string): Promise<void> {
  const db = await getDB();
  await db.delete("savedItems", id);
}

export async function removeItemByKey(key: string): Promise<void> {
  const db = await getDB();
  const item = await db.getFromIndex("savedItems", "by-key", key);
  if (item) {
    await db.delete("savedItems", item.id);
  }
}

export async function isItemSaved(
  type: SavedItemType,
  data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
): Promise<boolean> {
  const db = await getDB();
  const key = generateItemKey(type, data);
  const existing = await db.getFromIndex("savedItems", "by-key", key);
  return !!existing;
}

export async function getAllSavedItems(): Promise<SavedItem[]> {
  const db = await getDB();
  const items = await db.getAllFromIndex("savedItems", "by-savedAt");
  return items.reverse(); // Most recent first
}

export async function getSavedItemsByType(
  type: SavedItemType
): Promise<SavedItem[]> {
  const db = await getDB();
  const items = await db.getAllFromIndex("savedItems", "by-type", type);
  return items.sort((a, b) => b.savedAt - a.savedAt);
}

export async function getAllSavedKeys(): Promise<Set<string>> {
  const db = await getDB();
  const items = await db.getAll("savedItems");
  return new Set(items.map((item) => item.key));
}

// --- Search History ---

const MAX_HISTORY_ITEMS = 10;

function historyTermsMatch(a: Record<string, string>, b: Record<string, string>): boolean {
  const keysA = Object.keys(a);
  const keysB = Object.keys(b);
  if (keysA.length !== keysB.length) return false;
  return keysA.every((k) => a[k]?.toLowerCase() === b[k]?.toLowerCase());
}

export async function saveSearchToHistory(
  type: SearchHistoryItem["type"],
  terms: Record<string, string>
): Promise<void> {
  const db = await getDB();

  // Remove existing entry with same type+terms (dedup)
  const all = await db.getAllFromIndex("searchHistory", "by-timestamp");
  const existing = all.find((h) => h.type === type && historyTermsMatch(h.terms, terms));
  if (existing) {
    await db.delete("searchHistory", existing.id);
  }

  const item: SearchHistoryItem = {
    id: crypto.randomUUID(),
    type,
    terms,
    timestamp: Date.now(),
  };

  await db.add("searchHistory", item);

  // Keep only the last MAX_HISTORY_ITEMS entries
  const updated = await db.getAllFromIndex("searchHistory", "by-timestamp");
  if (updated.length > MAX_HISTORY_ITEMS) {
    const toDelete = updated.slice(0, updated.length - MAX_HISTORY_ITEMS);
    const tx = db.transaction("searchHistory", "readwrite");
    for (const old of toDelete) {
      tx.store.delete(old.id);
    }
    await tx.done;
  }
}

export async function getSearchHistory(): Promise<SearchHistoryItem[]> {
  const db = await getDB();
  const items = await db.getAllFromIndex("searchHistory", "by-timestamp");
  return items.reverse(); // Most recent first
}

export async function clearSearchHistory(): Promise<void> {
  const db = await getDB();
  await db.clear("searchHistory");
}

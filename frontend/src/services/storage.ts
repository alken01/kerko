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
}

const DB_NAME = "kerko-db";
const DB_VERSION = 1;

let dbPromise: Promise<IDBPDatabase<KerkoDBSchema>> | null = null;

function getDB(): Promise<IDBPDatabase<KerkoDBSchema>> {
  if (!dbPromise) {
    dbPromise = openDB<KerkoDBSchema>(DB_NAME, DB_VERSION, {
      upgrade(db) {
        const store = db.createObjectStore("savedItems", { keyPath: "id" });
        store.createIndex("by-type", "type");
        store.createIndex("by-savedAt", "savedAt");
        store.createIndex("by-key", "key", { unique: true });
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

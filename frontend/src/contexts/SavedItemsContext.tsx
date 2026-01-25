"use client";

import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
} from "react";
import {
  SavedItem,
  SavedItemType,
  generateItemKey,
} from "@/types/saved";
import {
  PatronazhistResponse,
  PersonResponse,
  RrogatResponse,
  TargatResponse,
} from "@/types/kerko";
import * as storage from "@/services/storage";

interface SavedItemsContextType {
  savedItems: SavedItem[];
  savedCount: number;
  savedKeys: Set<string>;
  saveItem: (
    type: SavedItemType,
    data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
  ) => Promise<boolean>;
  removeItem: (id: string) => Promise<void>;
  removeItemByKey: (key: string) => Promise<void>;
  isItemSaved: (
    type: SavedItemType,
    data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
  ) => boolean;
  refreshSavedItems: () => Promise<void>;
}

const SavedItemsContext = createContext<SavedItemsContextType | undefined>(
  undefined
);

export function SavedItemsProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const [savedItems, setSavedItems] = useState<SavedItem[]>([]);
  const [savedKeys, setSavedKeys] = useState<Set<string>>(new Set());
  const [mounted, setMounted] = useState(false);

  const refreshSavedItems = useCallback(async () => {
    try {
      const items = await storage.getAllSavedItems();
      const keys = await storage.getAllSavedKeys();
      setSavedItems(items);
      setSavedKeys(keys);
    } catch (error) {
      console.error("Failed to load saved items:", error);
    }
  }, []);

  useEffect(() => {
    refreshSavedItems().then(() => setMounted(true));
  }, [refreshSavedItems]);

  const saveItem = useCallback(
    async (
      type: SavedItemType,
      data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
    ): Promise<boolean> => {
      const item = await storage.saveItem(type, data);
      if (item) {
        setSavedItems((prev) => [item, ...prev]);
        setSavedKeys((prev) => new Set([...prev, item.key]));
        return true;
      }
      return false;
    },
    []
  );

  const removeItem = useCallback(async (id: string) => {
    const item = savedItems.find((i) => i.id === id);
    await storage.removeItem(id);
    setSavedItems((prev) => prev.filter((i) => i.id !== id));
    if (item) {
      setSavedKeys((prev) => {
        const newKeys = new Set(prev);
        newKeys.delete(item.key);
        return newKeys;
      });
    }
  }, [savedItems]);

  const removeItemByKey = useCallback(async (key: string) => {
    await storage.removeItemByKey(key);
    setSavedItems((prev) => prev.filter((i) => i.key !== key));
    setSavedKeys((prev) => {
      const newKeys = new Set(prev);
      newKeys.delete(key);
      return newKeys;
    });
  }, []);

  const isItemSaved = useCallback(
    (
      type: SavedItemType,
      data: PersonResponse | RrogatResponse | TargatResponse | PatronazhistResponse
    ): boolean => {
      const key = generateItemKey(type, data);
      return savedKeys.has(key);
    },
    [savedKeys]
  );

  if (!mounted) {
    return null;
  }

  return (
    <SavedItemsContext.Provider
      value={{
        savedItems,
        savedCount: savedItems.length,
        savedKeys,
        saveItem,
        removeItem,
        removeItemByKey,
        isItemSaved,
        refreshSavedItems,
      }}
    >
      {children}
    </SavedItemsContext.Provider>
  );
}

export function useSavedItems() {
  const context = useContext(SavedItemsContext);
  if (context === undefined) {
    throw new Error("useSavedItems must be used within a SavedItemsProvider");
  }
  return context;
}

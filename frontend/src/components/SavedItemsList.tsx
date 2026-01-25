"use client";

import { Bookmark, User, Banknote, Car, Users } from "lucide-react";
import { useSavedItems } from "@/contexts/SavedItemsContext";
import {
  PatronazhistResponse,
  PersonResponse,
  RrogatResponse,
  TargatResponse,
} from "@/types/kerko";
import { PersonCard } from "./PersonCard";
import { RrogatCard } from "./RrogatCard";
import { TargatCard } from "./TargatCard";
import { PatronazhistCard } from "./PatronazhistCard";
import { SavedItem, SavedItemType } from "@/types/saved";

interface SavedItemsListProps {
  onNameClick: (emri: string, mbiemri: string) => void;
}

const typeConfig: Record<SavedItemType, { label: string; icon: typeof User }> = {
  person: { label: "Persona", icon: User },
  rrogat: { label: "Paga", icon: Banknote },
  targat: { label: "Targa", icon: Car },
  patronazhist: { label: "Patronazhist", icon: Users },
};

export function SavedItemsList({ onNameClick }: SavedItemsListProps) {
  const { savedItems } = useSavedItems();

  if (savedItems.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="w-16 h-16 rounded-xl bg-surface-secondary flex items-center justify-center mb-5">
          <Bookmark className="h-8 w-8 text-accent-saved" />
        </div>
        <h3 className="text-lg font-medium text-text-primary mb-2">
          Asnjë e ruajtur
        </h3>
        <p className="text-text-secondary text-sm max-w-xs leading-relaxed">
          Shtypni ikonën <Bookmark className="inline h-4 w-4 mx-0.5 text-accent-saved" /> në rezultate për t&apos;i ruajtur këtu.
        </p>
      </div>
    );
  }

  // Group items by type
  const groupedItems = savedItems.reduce((acc, item) => {
    if (!acc[item.type]) acc[item.type] = [];
    acc[item.type].push(item);
    return acc;
  }, {} as Record<SavedItemType, SavedItem[]>);

  // Order: person, rrogat, targat, patronazhist
  const orderedTypes: SavedItemType[] = ["person", "rrogat", "targat", "patronazhist"];
  const activeTypes = orderedTypes.filter((type) => groupedItems[type]?.length > 0);

  return (
    <div className="flex flex-col gap-8 max-w-4xl mx-auto w-full">
      {activeTypes.map((type) => {
        const config = typeConfig[type];
        const Icon = config.icon;
        const items = groupedItems[type];

        return (
          <section key={type}>
            <div className="flex items-center gap-2 mb-4">
              <Icon className="h-4 w-4 text-text-tertiary" />
              <h3 className="text-sm font-medium text-text-secondary uppercase tracking-wide">
                {config.label}
              </h3>
              <span className="text-xs text-text-tertiary">
                ({items.length})
              </span>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {items.map((item) => {
                switch (item.type) {
                  case "person":
                    return (
                      <PersonCard
                        key={item.id}
                        person={item.data as PersonResponse}
                      />
                    );
                  case "rrogat":
                    return (
                      <RrogatCard
                        key={item.id}
                        rrogat={item.data as RrogatResponse}
                      />
                    );
                  case "targat":
                    return (
                      <TargatCard
                        key={item.id}
                        targat={item.data as TargatResponse}
                        onNameClick={onNameClick}
                      />
                    );
                  case "patronazhist":
                    return (
                      <PatronazhistCard
                        key={item.id}
                        patronazhist={item.data as PatronazhistResponse}
                      />
                    );
                  default:
                    return null;
                }
              })}
            </div>
          </section>
        );
      })}
    </div>
  );
}

"use client";

import { Bookmark } from "lucide-react";
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

interface SavedItemsListProps {
  onNameClick: (emri: string, mbiemri: string) => void;
}

export function SavedItemsList({ onNameClick }: SavedItemsListProps) {
  const { savedItems } = useSavedItems();

  if (savedItems.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <div className="w-16 h-16 rounded-full bg-surface-secondary flex items-center justify-center mb-4">
          <Bookmark className="h-8 w-8 text-text-tertiary" />
        </div>
        <h3 className="text-lg font-medium text-text-primary mb-2">
          Nuk keni të ruajtura
        </h3>
        <p className="text-text-secondary text-sm max-w-sm">
          Klikoni ikonën e bookmark në çdo kartë për të ruajtur rezultate.
        </p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
        {savedItems.map((item) => {
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
    </div>
  );
}

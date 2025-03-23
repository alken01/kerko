"use client";

import { useState } from "react";
import { SearchForm } from "@/components/SearchForm";
import { PersonDetails } from "@/components/PersonDetails";
import { Person } from "@/types/person";
import { Alert, AlertDescription } from "@/components/ui/alert";

export default function Home() {
  const [person, setPerson] = useState<Person | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = async (firstName: string, lastName: string) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetch(
        `/api/search?firstName=${encodeURIComponent(
          firstName
        )}&lastName=${encodeURIComponent(lastName)}`
      );

      if (!response.ok) {
        throw new Error("Failed to fetch person data");
      }

      const data = await response.json();
      setPerson(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An error occurred while searching"
      );
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#120404] overflow-x-hidden touch-manipulation dark">
      <style jsx global>{`
        body {
          overscroll-behavior: none;
          touch-action: manipulation;
          -webkit-tap-highlight-color: transparent;
        }
        @supports (-webkit-touch-callout: none) {
          .min-h-screen {
            min-height: -webkit-fill-available;
          }
        }
      `}</style>
      <main className="container py-8 space-y-4">
        <h1 className="text-center text-3xl font-bold tracking-tight text-white">
          Kërko
        </h1>

        <SearchForm onSearch={handleSearch} isLoading={isLoading} />

        {error && (
          <Alert
            variant="destructive"
            className="max-w-md mx-auto bg-[#2a0f0f] border-[#aa0000] text-[#ff4040]"
          >
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {person && <PersonDetails person={person} />}
      </main>
    </div>
  );
}

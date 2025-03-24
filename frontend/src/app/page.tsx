"use client";

import { useState } from "react";
import { SearchForm } from "@/components/SearchForm";
import { PersonCard } from "@/components/PersonCard";
import { RrogatCard } from "@/components/RrogatCard";
import { TargatCard } from "@/components/TargatCard";
import { PatronazhistCard } from "@/components/PatronazhistCard";
import { SearchResponse } from "@/types/kerko";
import { Alert, AlertDescription } from "@/components/ui/alert";

type TabType = "person" | "rrogat" | "targat" | "patronazhist";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5120";

export default function Home() {
  const [searchResults, setSearchResults] = useState<SearchResponse | null>(
    null
  );
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>("person");

  const handleSearch = async (emri: string, mbiemri: string) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);

    try {
      const response = await fetch(
        `${API_URL}/api/kerko?emri=${encodeURIComponent(
          emri
        )}&mbiemri=${encodeURIComponent(mbiemri)}`
      );

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error("Nuk u gjet asnjë rezultat");
        }
        throw new Error("Pati një problem gjatë kërkimit");
      }

      const data = await response.json();
      if (!data) {
        throw new Error("Nuk u gjet asnjë rezultat");
      }

      setSearchResults(data);
    } catch (err) {
      console.error("Search error:", err);
      setError(
        err instanceof Error ? err.message : "Pati një problem gjatë kërkimit"
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearchTarga = async (numriTarges: string) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);

    try {
      const response = await fetch(
        `${API_URL}/api/targat?numriTarges=${encodeURIComponent(numriTarges)}`
      );

      if (!response.ok) {
        throw new Error("Failed to fetch search results");
      }

      const data = await response.json();
      if (!data) {
        throw new Error("Nuk u gjet asnjë rezultat");
      }

      setSearchResults({
        person: [],
        rrogat: [],
        targat: [data],
        patronazhist: [],
      });
    } catch (err) {
      console.error("Search error:", err);
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  const handleNameClick = (emri: string, mbiemri: string) => {
    handleSearch(emri, mbiemri);
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
        ::-webkit-scrollbar {
          display: none;
        }
        * {
          -ms-overflow-style: none;
          scrollbar-width: none;
        }
      `}</style>
      <main className="container mx-auto py-8 space-y-6 px-4 md:px-6">
        <h1 className="text-center text-3xl font-bold tracking-tight text-white">
          Kërko
        </h1>

        <div className="max-w-md mx-auto">
          <SearchForm
            onSearch={handleSearch}
            onSearchTarga={handleSearchTarga}
            isLoading={isLoading}
          />
        </div>

        {error && (
          <Alert
            variant="destructive"
            className="max-w-md mx-auto bg-[#2a0f0f] border-[#aa0000] text-[#ff4040]"
          >
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {searchResults && (
          <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
            <div className="flex flex-wrap space-x-0 space-y-1 sm:space-y-0 sm:space-x-1 p-1 bg-[#120606] rounded-lg border border-[#2a1a1a]">
              {(
                ["person", "rrogat", "targat", "patronazhist"] as TabType[]
              ).map((type, index) => (
                <button
                  key={type}
                  onClick={() => setActiveTab(type)}
                  className={`w-full sm:flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                    index > 0 ? "mt-1 sm:mt-0" : ""
                  } ${
                    activeTab === type
                      ? "bg-[#2a1a1a] text-white shadow-sm"
                      : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
                  }`}
                >
                  <div className="flex items-center justify-center gap-2">
                    <span>
                      {type === "person"
                        ? "Persona"
                        : type === "rrogat"
                        ? "Rrogat"
                        : type === "targat"
                        ? "Targat"
                        : "Patronazhist"}
                    </span>
                    <span className="text-xs bg-[#1a1a1a] px-2 py-0.5 rounded-full">
                      {searchResults[type]?.length || 0}
                    </span>
                  </div>
                </button>
              ))}
            </div>

            <div className="relative overflow-hidden">
              {activeTab === "person" && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
                  {searchResults.person?.map((person, index) => (
                    <PersonCard key={index} person={person} />
                  ))}
                </div>
              )}
              {activeTab === "rrogat" && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
                  {searchResults.rrogat?.map((rrogat, index) => (
                    <RrogatCard key={index} rrogat={rrogat} />
                  ))}
                </div>
              )}
              {activeTab === "targat" && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
                  {searchResults.targat?.map((targat, index) => (
                    <TargatCard
                      key={index}
                      targat={targat}
                      onNameClick={handleNameClick}
                    />
                  ))}
                </div>
              )}
              {activeTab === "patronazhist" && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
                  {searchResults.patronazhist?.map((patronazhist, index) => (
                    <PatronazhistCard key={index} patronazhist={patronazhist} />
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </main>
    </div>
  );
}

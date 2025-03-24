"use client";

import { useState } from "react";
import { SearchForm } from "@/components/SearchForm";
import { SearchResultsTabs } from "@/components/SearchResultsTabs";
import { GlobalStyles } from "@/components/GlobalStyles";
import { SearchResponse } from "@/types/kerko";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { ApiService } from "@/services/api";

type TabType = "person" | "rrogat" | "targat" | "patronazhist";

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
      const data = await ApiService.searchPerson(emri, mbiemri);
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
      const data = await ApiService.searchTarga(numriTarges);
      setSearchResults(data);
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
    <div className="min-h-screen bg-[#0a0303] overflow-x-hidden touch-manipulation dark">
      <GlobalStyles />
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
          <SearchResultsTabs
            searchResults={searchResults}
            activeTab={activeTab}
            onTabChange={setActiveTab}
            onNameClick={handleNameClick}
          />
        )}
      </main>
    </div>
  );
}

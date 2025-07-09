"use client";

import { useState, useEffect, Suspense } from "react";
import { SearchForm } from "@/components/SearchForm";
import { SearchResultsTabs } from "@/components/SearchResultsTabs";
import { GlobalStyles } from "@/components/GlobalStyles";
import { SearchResponse } from "@/types/kerko";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { ApiService } from "@/services/api";
import { useSearchParams } from "next/navigation";

type TabType = "person" | "rrogat" | "targat" | "patronazhist";

function SearchContent() {
  const searchParams = useSearchParams();
  const [searchResults, setSearchResults] = useState<SearchResponse | null>(
    null
  );
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>("person");
  const [isTargaSearch, setIsTargaSearch] = useState(false);
  const [isTelefonSearch, setIsTelefonSearch] = useState(false);
  const [searchFormData, setSearchFormData] = useState<{
    emri: string;
    mbiemri: string;
  } | null>(null);

  // Handle URL parameters on initial load
  useEffect(() => {
    const emri = searchParams.get("emri");
    const mbiemri = searchParams.get("mbiemri");
    const targa = searchParams.get("targa");
    const telefon = searchParams.get("telefon");

    if (targa) {
      handleSearchTarga(targa);
    } else if (telefon) {
      handleSearchTelefon(telefon);
    } else if (emri && mbiemri) {
      setSearchFormData({ emri, mbiemri });
      handleSearch(emri, mbiemri);
    }
  }, [searchParams]);

  const handleSearch = async (emri: string, mbiemri: string) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);
    setIsTargaSearch(false);
    setIsTelefonSearch(false);
    setActiveTab("person");

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
    setActiveTab("targat");
    setIsTargaSearch(true);
    setIsTelefonSearch(false);

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

  const handleSearchTelefon = async (numriTelefonit: string) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);
    setActiveTab("patronazhist");
    setIsTargaSearch(false);
    setIsTelefonSearch(true);

    try {
      const data = await ApiService.searchTelefon(numriTelefonit);
      setSearchResults(data);
    } catch (err) {
      console.error("Search error:", err);
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  const handleNameClick = (emri: string, mbiemri: string) => {
    setSearchFormData({ emri, mbiemri });
    handleSearch(emri, mbiemri);
  };

  const handleClear = () => {
    setSearchResults(null);
    setError(null);
    setIsTargaSearch(false);
    setIsTelefonSearch(false);
  };

  return (
    <>
      <div className="max-w-md mx-auto">
        <SearchForm
          onSearch={handleSearch}
          onSearchTarga={handleSearchTarga}
          onSearchTelefon={handleSearchTelefon}
          onClear={handleClear}
          isLoading={isLoading}
          defaultValues={searchFormData}
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
          isTargaSearch={isTargaSearch}
          isTelefonSearch={isTelefonSearch}
        />
      )}
    </>
  );
}

export default function Home() {
  return (
    <div className="min-h-screen bg-[#0a0303] overflow-x-hidden touch-manipulation dark">
      <GlobalStyles />
      <main className="container mx-auto py-6 space-y-4 px-4 md:px-6">
        <h1 className="text-center text-3xl font-bold tracking-tight text-white">
          Kërko
        </h1>
        <Suspense
          fallback={<div className="text-white text-center">Loading...</div>}
        >
          <SearchContent />
        </Suspense>
      </main>
    </div>
  );
}

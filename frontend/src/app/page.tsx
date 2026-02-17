"use client";

import { useState, useEffect, Suspense } from "react";
import { SearchForm } from "@/components/SearchForm";
import { SearchResultsTabs } from "@/components/SearchResultsTabs";
import { SavedItemsPanel } from "@/components/SavedItemsPanel";
import { GlobalStyles } from "@/components/GlobalStyles";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { SearchResponse, TargatSearchResponse, PatronazhistSearchResponse, TabType } from "@/types/kerko";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { ApiService } from "@/services/api";
import { useSearchParams } from "next/navigation";
import { SEARCH_ERROR_MESSAGE } from "@/lib/constants";

function SearchContent() {
  const searchParams = useSearchParams();
  const [searchResults, setSearchResults] = useState<SearchResponse | TargatSearchResponse | PatronazhistSearchResponse | null>(
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
  const [currentSearchTerms, setCurrentSearchTerms] = useState<{
    type: 'person' | 'targa' | 'telefon';
    terms: string[];
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

  const handleSearch = async (emri: string, mbiemri: string, page: number = 1) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);
    setIsTargaSearch(false);
    setIsTelefonSearch(false);
    setActiveTab("person");
    setCurrentSearchTerms({ type: 'person', terms: [emri, mbiemri] });

    try {
      const data = await ApiService.searchPerson(emri, mbiemri, page);
      setSearchResults(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : SEARCH_ERROR_MESSAGE);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearchTarga = async (numriTarges: string, page: number = 1) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);
    setActiveTab("targat");
    setIsTargaSearch(true);
    setIsTelefonSearch(false);
    setCurrentSearchTerms({ type: 'targa', terms: [numriTarges] });

    try {
      const data = await ApiService.searchTarga(numriTarges, page);
      setSearchResults(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : SEARCH_ERROR_MESSAGE);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearchTelefon = async (numriTelefonit: string, page: number = 1) => {
    setIsLoading(true);
    setError(null);
    setSearchResults(null);
    setActiveTab("patronazhist");
    setIsTargaSearch(false);
    setIsTelefonSearch(true);
    setCurrentSearchTerms({ type: 'telefon', terms: [numriTelefonit] });

    try {
      const data = await ApiService.searchTelefon(numriTelefonit, page);
      setSearchResults(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : SEARCH_ERROR_MESSAGE);
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

  const handlePageChange = (page: number) => {
    if (!currentSearchTerms) return;

    switch (currentSearchTerms.type) {
      case 'person':
        handleSearch(currentSearchTerms.terms[0], currentSearchTerms.terms[1], page);
        break;
      case 'targa':
        handleSearchTarga(currentSearchTerms.terms[0], page);
        break;
      case 'telefon':
        handleSearchTelefon(currentSearchTerms.terms[0], page);
        break;
    }
  };

  return (
    <>
      <div className="max-w-md mx-auto">
        <SearchForm
          onClear={handleClear}
          isLoading={isLoading}
          defaultValues={searchFormData}
        />
      </div>

      {error && (
        <Alert
          variant="destructive"
          className="max-w-md mx-auto"
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
          onPageChange={handlePageChange}
          isTargaSearch={isTargaSearch}
          isTelefonSearch={isTelefonSearch}
          searchTerms={currentSearchTerms?.type === 'person' ? { emri: currentSearchTerms.terms[0], mbiemri: currentSearchTerms.terms[1] } : undefined}
        />
      )}

      <SavedItemsPanel onNameClick={handleNameClick} />
    </>
  );
}

export default function Home() {
  return (
    <div className="min-h-screen bg-surface-primary overflow-x-hidden touch-manipulation">
      <GlobalStyles />

      <main className="container mx-auto py-6 space-y-4 px-4 md:px-6">
        {/* Title with theme toggle on same line */}
        <div className="flex items-center justify-center relative">
          <h1 className="text-3xl font-bold tracking-tight text-text-primary">
            Kërko
          </h1>
          <div className="absolute right-0">
            <ThemeToggle />
          </div>
        </div>

        <Suspense
          fallback={<div className="text-text-primary text-center">Loading...</div>}
        >
          <SearchContent />
        </Suspense>
      </main>
    </div>
  );
}

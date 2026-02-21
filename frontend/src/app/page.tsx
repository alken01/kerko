"use client";

import { useState, useEffect, Suspense } from "react";
import { SearchForm } from "@/components/SearchForm";
import { SearchResultsTabs } from "@/components/SearchResultsTabs";
import { SavedItemsPanel } from "@/components/SavedItemsPanel";
import { GlobalStyles } from "@/components/GlobalStyles";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { LanguageSwitcher } from "@/components/ui/language-switcher";
import { SkeletonGrid } from "@/components/SkeletonCard";
import { SearchResponse, TargatSearchResponse, PatronazhistSearchResponse, TabType } from "@/types/kerko";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { ApiService } from "@/services/api";
import { useSearchParams } from "next/navigation";
import { useTranslation } from "@/i18n/TranslationContext";
import { SEARCH_ERROR_KEY } from "@/lib/constants";

function SearchContent() {
  const searchParams = useSearchParams();
  const { t } = useTranslation();
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
  const [maidenNameHint, setMaidenNameHint] = useState(false);

  // Auto-dismiss errors after 5 seconds
  useEffect(() => {
    if (!error) return;
    const timer = setTimeout(() => setError(null), 5000);
    return () => clearTimeout(timer);
  }, [error]);

  // Handle URL parameters on initial load
  useEffect(() => {
    const emri = searchParams.get("emri");
    const mbiemri = searchParams.get("mbiemri");
    const targa = searchParams.get("targa");
    const telefon = searchParams.get("telefon");
    const hint = searchParams.get("hint");

    setMaidenNameHint(hint === "mbiemri");

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
    if (page === 1 && !searchParams.get("hint")) setMaidenNameHint(false);
    setIsTargaSearch(false);
    setIsTelefonSearch(false);
    setActiveTab("person");
    setCurrentSearchTerms({ type: 'person', terms: [emri, mbiemri] });

    try {
      const data = await ApiService.searchPerson(emri, mbiemri, page);
      setSearchResults(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : SEARCH_ERROR_KEY);
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
      setError(err instanceof Error ? err.message : SEARCH_ERROR_KEY);
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
      setError(err instanceof Error ? err.message : SEARCH_ERROR_KEY);
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

      {isLoading && <SkeletonGrid count={4} />}

      {error && !isLoading && (
        <Alert
          variant="destructive"
          className="max-w-md mx-auto"
        >
          <AlertDescription>{t(error)}</AlertDescription>
        </Alert>
      )}

      {maidenNameHint && searchResults && !isLoading && (
        <div className="flex justify-center">
          <div className="w-fit rounded-lg border border-yellow-500/50 bg-yellow-50 dark:bg-yellow-950/20 px-4 py-2 text-center">
            <p className="text-yellow-800 dark:text-yellow-200 text-xs">
              {t("search.maidenNameHint")}
            </p>
          </div>
        </div>
      )}

      {searchResults && !isLoading && (
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
  const { t } = useTranslation();

  return (
    <div className="min-h-screen bg-surface-primary overflow-x-hidden touch-manipulation">
      <GlobalStyles />

      <main className="container mx-auto py-6 space-y-4 px-4 md:px-6">
        {/* Title with language and theme toggles */}
        <div className="max-w-md mx-auto flex items-center justify-center relative">
          <h1 className="text-3xl font-bold tracking-tight text-text-primary">
            {t("app.title")}
          </h1>
          <div className="absolute right-0 flex items-center gap-2">
            <LanguageSwitcher />
            <ThemeToggle />
          </div>
        </div>

        <Suspense
          fallback={<div className="text-text-primary text-center">{t("app.loading")}</div>}
        >
          <SearchContent />
        </Suspense>
      </main>
    </div>
  );
}

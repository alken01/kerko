import {
  PatronazhistSearchResponse,
  SearchResponse,
  TargatSearchResponse,
  TabType,
} from "@/types/kerko";
import { ChevronDown, ChevronUp } from "lucide-react";
import { useState } from "react";
import { Pagination } from "./Pagination";
import { PatronazhistCard } from "./PatronazhistCard";
import { PersonCard } from "./PersonCard";
import { RrogatCard } from "./RrogatCard";
import { TargatCard } from "./TargatCard";

function normalizeAlbanian(str: string): string {
  return str.toLowerCase().replace(/ç/g, "c").replace(/ë/g, "e");
}

function splitByRelevance<T extends { emri: string | null; mbiemri: string | null }>(
  items: T[],
  searchEmri: string,
  searchMbiemri: string
): { exact: T[]; similar: T[] } {
  const normEmri = normalizeAlbanian(searchEmri);
  const normMbiemri = normalizeAlbanian(searchMbiemri);

  const exact: T[] = [];
  const similar: T[] = [];

  for (const item of items) {
    const itemEmri = normalizeAlbanian(item.emri || "");
    const itemMbiemri = normalizeAlbanian(item.mbiemri || "");
    if (itemEmri === normEmri && itemMbiemri === normMbiemri) {
      exact.push(item);
    } else {
      similar.push(item);
    }
  }

  return { exact, similar };
}

function GroupedResultsGrid<T extends { emri: string | null; mbiemri: string | null }>({
  items,
  searchTerms,
  renderItem,
}: {
  items: T[] | undefined;
  searchTerms?: { emri: string; mbiemri: string };
  renderItem: (item: T, index: number) => React.ReactNode;
}) {
  const [showSimilar, setShowSimilar] = useState(true);

  if (!items || items.length === 0) return null;

  // If no search terms (e.g. targa/telefon search), render all items flat
  if (!searchTerms) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
        {items.map((item, index) => renderItem(item, index))}
      </div>
    );
  }

  const { exact, similar } = splitByRelevance(items, searchTerms.emri, searchTerms.mbiemri);

  // If all results are exact or all are similar, render flat
  if (similar.length === 0) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
        {exact.map((item, index) => renderItem(item, index))}
      </div>
    );
  }

  if (exact.length === 0) {
    return (
      <div className="space-y-3 animate-in fade-in duration-200">
        <p className="text-sm text-text-tertiary text-center">
          Nuk u gjet rezultat ekzakt. Rezultate te ngjashme:
        </p>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
          {similar.map((item, index) => renderItem(item, index))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4 animate-in fade-in duration-200">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
        {exact.map((item, index) => renderItem(item, index))}
      </div>

      <button
        onClick={() => setShowSimilar(!showSimilar)}
        className="w-full flex items-center gap-2 py-2 group"
      >
        <div className="flex-1 h-px bg-border-semantic-secondary" />
        <span className="text-sm text-text-tertiary group-hover:text-text-secondary transition-colors flex items-center gap-1">
          Rezultate te ngjashme ({similar.length})
          {showSimilar ? (
            <ChevronUp className="h-4 w-4" />
          ) : (
            <ChevronDown className="h-4 w-4" />
          )}
        </span>
        <div className="flex-1 h-px bg-border-semantic-secondary" />
      </button>

      {showSimilar && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
          {similar.map((item, index) => renderItem(item, exact.length + index))}
        </div>
      )}
    </div>
  );
}

type SearchResultsType = SearchResponse | TargatSearchResponse | PatronazhistSearchResponse;

function isSearchResponse(response: SearchResultsType): response is SearchResponse {
  return "person" in response;
}

function hasDirectItems(response: SearchResultsType): boolean {
  return "items" in response && !("person" in response);
}

function asTargatResponse(response: SearchResultsType): TargatSearchResponse {
  return response as TargatSearchResponse;
}

function asPatronazhistResponse(response: SearchResultsType): PatronazhistSearchResponse {
  return response as PatronazhistSearchResponse;
}

interface SearchResultsTabsProps {
  searchResults:
    | SearchResponse
    | TargatSearchResponse
    | PatronazhistSearchResponse;
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
  onNameClick: (emri: string, mbiemri: string) => void;
  onPageChange: (page: number) => void;
  isTargaSearch: boolean;
  isTelefonSearch: boolean;
  searchTerms?: { emri: string; mbiemri: string };
}

export function SearchResultsTabs({
  searchResults,
  activeTab,
  onTabChange,
  onNameClick,
  onPageChange,
  isTargaSearch,
  isTelefonSearch,
  searchTerms,
}: SearchResultsTabsProps) {
  const tabs = isTargaSearch
    ? [{ value: "targat", label: "Targat" }]
    : isTelefonSearch
    ? [{ value: "patronazhist", label: "Patronazhistë" }]
    : [
        { value: "person", label: "Persona" },
        { value: "rrogat", label: "Rrogat" },
        { value: "targat", label: "Targat" },
        { value: "patronazhist", label: "Patronazhistë" },
      ];

  const getCurrentPagination = () => {
    if (isTargaSearch && hasDirectItems(searchResults)) {
      return asTargatResponse(searchResults).pagination;
    }
    if (isTelefonSearch && hasDirectItems(searchResults)) {
      return asPatronazhistResponse(searchResults).pagination;
    }
    if (isSearchResponse(searchResults)) {
      return searchResults[activeTab as keyof SearchResponse]?.pagination;
    }
    return null;
  };

  return (
    <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
      <div className="flex space-x-1 p-1 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary overflow-x-auto">
        {tabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => onTabChange(tab.value as TabType)}
            className={`flex-1 flex-shrink-0 min-w-fit px-3 py-2 text-xs font-medium rounded-md transition-all duration-200 ${
              activeTab === tab.value
                ? "bg-surface-tertiary text-text-primary "
                : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
            }`}
          >
            <div className="flex items-center justify-center gap-1">
              <span>{tab.label}</span>
              <span className="text-xs bg-surface-interactive px-1.5 py-0.5 rounded-full">
                {(() => {
                  if (isTargaSearch && hasDirectItems(searchResults)) {
                    return asTargatResponse(searchResults).pagination?.totalItems || 0;
                  }
                  if (isTelefonSearch && hasDirectItems(searchResults)) {
                    return asPatronazhistResponse(searchResults).pagination?.totalItems || 0;
                  }
                  if (isSearchResponse(searchResults)) {
                    return (
                      searchResults[tab.value as keyof SearchResponse]
                        ?.pagination?.totalItems || 0
                    );
                  }
                  return 0;
                })()}
              </span>
            </div>
          </button>
        ))}
      </div>

      <div className="relative overflow-hidden">
        {activeTab === "person" && isSearchResponse(searchResults) && (
          <GroupedResultsGrid
            items={searchResults.person?.items}
            searchTerms={searchTerms}
            renderItem={(person, index) => <PersonCard key={index} person={person} />}
          />
        )}
        {activeTab === "rrogat" && isSearchResponse(searchResults) && (
          <GroupedResultsGrid
            items={searchResults.rrogat?.items}
            searchTerms={searchTerms}
            renderItem={(rrogat, index) => <RrogatCard key={index} rrogat={rrogat} />}
          />
        )}
        {activeTab === "targat" && (
          <GroupedResultsGrid
            items={
              isTargaSearch && hasDirectItems(searchResults)
                ? asTargatResponse(searchResults).items
                : isSearchResponse(searchResults)
                ? searchResults.targat?.items
                : undefined
            }
            searchTerms={searchTerms}
            renderItem={(targat, index) => (
              <TargatCard key={index} targat={targat} onNameClick={onNameClick} />
            )}
          />
        )}
        {activeTab === "patronazhist" && (
          <GroupedResultsGrid
            items={
              isTelefonSearch && hasDirectItems(searchResults)
                ? asPatronazhistResponse(searchResults).items
                : isSearchResponse(searchResults)
                ? searchResults.patronazhist?.items
                : undefined
            }
            searchTerms={searchTerms}
            renderItem={(patronazhist, index) => (
              <PatronazhistCard key={index} patronazhist={patronazhist} />
            )}
          />
        )}
      </div>

      {(() => {
        const pagination = getCurrentPagination();
        return (
          pagination && (
            <Pagination pagination={pagination} onPageChange={onPageChange} />
          )
        );
      })()}
    </div>
  );
}

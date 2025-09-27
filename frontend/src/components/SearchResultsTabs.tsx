import { SearchResponse, TargatSearchResponse, PatronazhistSearchResponse } from "@/types/kerko";
import { PersonCard } from "./PersonCard";
import { RrogatCard } from "./RrogatCard";
import { TargatCard } from "./TargatCard";
import { PatronazhistCard } from "./PatronazhistCard";
import { Pagination } from "./Pagination";

type TabType = "person" | "rrogat" | "targat" | "patronazhist";

interface SearchResultsTabsProps {
  searchResults: SearchResponse | TargatSearchResponse | PatronazhistSearchResponse;
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
  onNameClick: (emri: string, mbiemri: string) => void;
  onPageChange: (page: number) => void;
  isTargaSearch: boolean;
  isTelefonSearch: boolean;
}

export function SearchResultsTabs({
  searchResults,
  activeTab,
  onTabChange,
  onNameClick,
  onPageChange,
  isTargaSearch,
  isTelefonSearch,
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
    if (isTargaSearch && 'pagination' in searchResults) {
      return searchResults.pagination;
    }
    if (isTelefonSearch && 'pagination' in searchResults) {
      return searchResults.pagination;
    }
    if ('person' in searchResults) {
      return searchResults[activeTab as keyof SearchResponse]?.pagination;
    }
    return null;
  };

  return (
    <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
      <div className="flex space-x-1 p-1 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary">
        {tabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => onTabChange(tab.value as TabType)}
            className={`flex-1 px-2 py-2 text-xs font-medium rounded-md transition-all duration-200 ${
              activeTab === tab.value
                ? "bg-surface-tertiary text-text-primary dark:shadow-sm"
                : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
            }`}
          >
            <div className="flex items-center justify-center gap-1">
              <span>{tab.label}</span>
              <span className="text-xs bg-surface-interactive px-1.5 py-0.5 rounded-full">
                {(() => {
                  if (isTargaSearch && 'pagination' in searchResults) {
                    return searchResults.pagination?.totalItems || 0;
                  }
                  if (isTelefonSearch && 'pagination' in searchResults) {
                    return searchResults.pagination?.totalItems || 0;
                  }
                  if ('person' in searchResults) {
                    return searchResults[tab.value as keyof SearchResponse]?.pagination?.totalItems || 0;
                  }
                  return 0;
                })()}
              </span>
            </div>
          </button>
        ))}
      </div>

      <div className="relative overflow-hidden">
        {activeTab === "person" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
            {(() => {
              if ('person' in searchResults) {
                return searchResults.person?.items?.map((person, index) => (
                  <PersonCard key={index} person={person} />
                ));
              }
              return null;
            })()}
          </div>
        )}
        {activeTab === "rrogat" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
            {(() => {
              if ('rrogat' in searchResults) {
                return searchResults.rrogat?.items?.map((rrogat, index) => (
                  <RrogatCard key={index} rrogat={rrogat} />
                ));
              }
              return null;
            })()}
          </div>
        )}
        {activeTab === "targat" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
            {(() => {
              if (isTargaSearch && 'items' in searchResults && !('person' in searchResults)) {
                return (searchResults as TargatSearchResponse).items?.map((targat, index) => (
                  <TargatCard
                    key={index}
                    targat={targat}
                    onNameClick={onNameClick}
                  />
                ));
              }
              if ('targat' in searchResults) {
                return searchResults.targat?.items?.map((targat, index) => (
                  <TargatCard
                    key={index}
                    targat={targat}
                    onNameClick={onNameClick}
                  />
                ));
              }
              return null;
            })()}
          </div>
        )}
        {activeTab === "patronazhist" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in duration-200">
            {(() => {
              if (isTelefonSearch && 'items' in searchResults && !('person' in searchResults)) {
                return (searchResults as PatronazhistSearchResponse).items?.map((patronazhist, index) => (
                  <PatronazhistCard key={index} patronazhist={patronazhist} />
                ));
              }
              if ('patronazhist' in searchResults) {
                return searchResults.patronazhist?.items?.map((patronazhist, index) => (
                  <PatronazhistCard key={index} patronazhist={patronazhist} />
                ));
              }
              return null;
            })()}
          </div>
        )}
      </div>

      {(() => {
        const pagination = getCurrentPagination();
        return pagination && (
          <Pagination
            pagination={pagination}
            onPageChange={onPageChange}
          />
        );
      })()}
    </div>
  );
}

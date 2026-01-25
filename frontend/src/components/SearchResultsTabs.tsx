import {
  PatronazhistSearchResponse,
  SearchResponse,
  TargatSearchResponse,
  TabType,
} from "@/types/kerko";
import { Pagination } from "./Pagination";
import { PatronazhistCard } from "./PatronazhistCard";
import { PersonCard } from "./PersonCard";
import { RrogatCard } from "./RrogatCard";
import { TargatCard } from "./TargatCard";

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
      <div className="flex space-x-1 p-1 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary">
        {tabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => onTabChange(tab.value as TabType)}
            className={`flex-1 px-2 py-2 text-xs font-medium rounded-md transition-all duration-200 ${
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
        {activeTab === "person" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
            {isSearchResponse(searchResults) &&
              searchResults.person?.items?.map((person, index) => (
                <PersonCard key={index} person={person} />
              ))}
          </div>
        )}
        {activeTab === "rrogat" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
            {isSearchResponse(searchResults) &&
              searchResults.rrogat?.items?.map((rrogat, index) => (
                <RrogatCard key={index} rrogat={rrogat} />
              ))}
          </div>
        )}
        {activeTab === "targat" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
            {(() => {
              if (isTargaSearch && hasDirectItems(searchResults)) {
                return asTargatResponse(searchResults).items?.map((targat, index) => (
                  <TargatCard
                    key={index}
                    targat={targat}
                    onNameClick={onNameClick}
                  />
                ));
              }
              if (isSearchResponse(searchResults)) {
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
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start animate-in fade-in duration-200">
            {(() => {
              if (isTelefonSearch && hasDirectItems(searchResults)) {
                return asPatronazhistResponse(searchResults).items?.map((patronazhist, index) => (
                  <PatronazhistCard key={index} patronazhist={patronazhist} />
                ));
              }
              if (isSearchResponse(searchResults)) {
                return searchResults.patronazhist?.items?.map(
                  (patronazhist, index) => (
                    <PatronazhistCard key={index} patronazhist={patronazhist} />
                  )
                );
              }
              return null;
            })()}
          </div>
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

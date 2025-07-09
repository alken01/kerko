import { SearchResponse } from "@/types/kerko";
import { PersonCard } from "./PersonCard";
import { RrogatCard } from "./RrogatCard";
import { TargatCard } from "./TargatCard";
import { PatronazhistCard } from "./PatronazhistCard";

type TabType = "person" | "rrogat" | "targat" | "patronazhist";

interface SearchResultsTabsProps {
  searchResults: SearchResponse;
  activeTab: TabType;
  onTabChange: (tab: TabType) => void;
  onNameClick: (emri: string, mbiemri: string) => void;
  isTargaSearch: boolean;
  isTelefonSearch: boolean;
}

export function SearchResultsTabs({
  searchResults,
  activeTab,
  onTabChange,
  onNameClick,
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

  return (
    <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
      <div className="flex space-x-1 p-1 bg-[#120606] rounded-lg border-2 border-[#2a1a1a]">
        {tabs.map((tab) => (
          <button
            key={tab.value}
            onClick={() => onTabChange(tab.value as TabType)}
            className={`flex-1 px-2 py-2 text-xs font-medium rounded-md transition-all duration-200 ${
              activeTab === tab.value
                ? "bg-[#2a1a1a] text-white shadow-sm"
                : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
            }`}
          >
            <div className="flex items-center justify-center gap-1">
              <span>{tab.label}</span>
              <span className="text-xs bg-[#1a1a1a] px-1.5 py-0.5 rounded-full">
                {searchResults[tab.value as keyof SearchResponse]?.length || 0}
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
                onNameClick={onNameClick}
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
  );
}

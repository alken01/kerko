"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { cardStyles } from "./ui/card-styles";

interface SearchFormProps {
  onSearch: (emri: string, mbiemri: string) => void;
  onSearchTarga: (numriTarges: string) => void;
  isLoading: boolean;
}

export function SearchForm({
  onSearch,
  onSearchTarga,
  isLoading,
}: SearchFormProps) {
  const [emri, setEmri] = useState("");
  const [mbiemri, setMbiemri] = useState("");
  const [numriTarges, setNumriTarges] = useState("");
  const [searchType, setSearchType] = useState<"name" | "plate">("name");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchType === "name") {
      onSearch(emri, mbiemri);
    } else {
      onSearchTarga(numriTarges);
    }
  };

  return (
    <Card className={cardStyles.root}>
      <CardContent className="p-5 space-y-4">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="flex flex-wrap space-x-0 space-y-1 sm:space-y-0 sm:space-x-1 p-1 bg-[#120606] rounded-lg border border-[#2a1a1a]">
            <button
              type="button"
              onClick={() => setSearchType("name")}
              className={`w-full sm:flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                searchType === "name"
                  ? "bg-[#2a1a1a] text-white shadow-sm"
                  : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
              }`}
            >
              Kërko sipas Emrit
            </button>
            <button
              type="button"
              onClick={() => setSearchType("plate")}
              className={`w-full sm:flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                searchType === "plate"
                  ? "bg-[#2a1a1a] text-white shadow-sm"
                  : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
              }`}
            >
              Kërko sipas Targës
            </button>
          </div>

          {searchType === "name" ? (
            <div className="space-y-2">
              <Input
                type="text"
                placeholder="Emri"
                value={emri}
                onChange={(e) => setEmri(e.target.value)}
                className="w-full bg-[#120606] border-2 border-[#2a1a1a] text-white placeholder:text-[#666] placeholder:font-normal focus-visible:ring-[#333] focus-visible:ring-offset-0 h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
                style={{ WebkitTapHighlightColor: "transparent" }}
                disabled={isLoading}
              />
              <Input
                type="text"
                placeholder="Mbiemri"
                value={mbiemri}
                onChange={(e) => setMbiemri(e.target.value)}
                className="w-full bg-[#120606] border-2 border-[#2a1a1a] text-white placeholder:text-[#666] placeholder:font-normal focus-visible:ring-[#333] focus-visible:ring-offset-0 h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
                style={{ WebkitTapHighlightColor: "transparent" }}
                disabled={isLoading}
              />
            </div>
          ) : (
            <div className="space-y-2">
              <Input
                type="text"
                placeholder="Numri i Targës"
                value={numriTarges}
                onChange={(e) => setNumriTarges(e.target.value)}
                className="w-full bg-[#120606] border-2 border-[#2a1a1a] text-white placeholder:text-[#666] placeholder:font-normal focus-visible:ring-[#333] focus-visible:ring-offset-0 h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
                style={{ WebkitTapHighlightColor: "transparent" }}
                disabled={isLoading}
              />
            </div>
          )}

          <div className="flex gap-3">
            <Button
              type="button"
              variant="outline"
              className="flex-1 bg-[#120606] border-2 border-[#2a1a1a] text-[#999] hover:bg-[#2a1a1a] hover:text-white h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
              style={{ WebkitTapHighlightColor: "transparent" }}
              onClick={() => {
                setEmri("");
                setMbiemri("");
                setNumriTarges("");
              }}
            >
              Pastro
            </Button>
            <Button
              type="submit"
              className="flex-1 bg-[#120606] border-2 border-[#2a1a1a] text-white hover:bg-[#2a1a1a] h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation disabled:opacity-50"
              style={{ WebkitTapHighlightColor: "transparent" }}
              disabled={isLoading}
            >
              {isLoading ? "Duke kërkuar..." : "Kërko"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

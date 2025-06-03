"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { cardStyles } from "./ui/card-styles";
import { useRouter, useSearchParams } from "next/navigation";

interface SearchFormProps {
  onSearch: (emri: string, mbiemri: string) => void;
  onSearchTarga: (numriTarges: string) => void;
  onClear: () => void;
  isLoading: boolean;
  defaultValues?: { emri: string; mbiemri: string } | null;
}

export function SearchForm({
  onSearch,
  onSearchTarga,
  onClear,
  isLoading,
  defaultValues,
}: SearchFormProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [activeTab, setActiveTab] = useState<"name" | "targa">("name");
  const [emri, setEmri] = useState("");
  const [mbiemri, setMbiemri] = useState("");
  const [targa, setTarga] = useState("");

  useEffect(() => {
    const urlTarga = searchParams.get("targa");
    if (urlTarga) {
      setActiveTab("targa");
      setTarga(urlTarga);
    } else if (defaultValues) {
      setActiveTab("name");
      setEmri(defaultValues.emri);
      setMbiemri(defaultValues.mbiemri);
    }
  }, [defaultValues, searchParams]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (activeTab === "name") {
      const params = new URLSearchParams(searchParams.toString());
      params.set("emri", emri);
      params.set("mbiemri", mbiemri);
      params.delete("targa");
      router.push(`?${params.toString()}`, { scroll: false });

      onSearch(emri, mbiemri);
    } else {
      const params = new URLSearchParams(searchParams.toString());
      params.set("targa", targa);
      params.delete("emri");
      params.delete("mbiemri");
      router.push(`?${params.toString()}`, { scroll: false });

      onSearchTarga(targa);
    }
  };

  const handleClear = () => {
    setEmri("");
    setMbiemri("");
    setTarga("");
    router.replace("/", { scroll: false });
    if (onClear) {
      onClear();
    }
  };

  return (
    <Card className={cardStyles.root}>
      <CardContent className="p-5 space-y-4">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="flex space-x-1 p-1 bg-[#120606] rounded-lg border-2 border-[#2a1a1a]">
            <button
              type="button"
              onClick={() => setActiveTab("name")}
              className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                activeTab === "name"
                  ? "bg-[#2a1a1a] text-white shadow-sm"
                  : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
              }`}
            >
              Emri
            </button>
            <button
              type="button"
              onClick={() => setActiveTab("targa")}
              className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                activeTab === "targa"
                  ? "bg-[#2a1a1a] text-white shadow-sm"
                  : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
              }`}
            >
              Targa
            </button>
          </div>

          {activeTab === "name" ? (
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
                placeholder="Targa"
                value={targa}
                onChange={(e) => setTarga(e.target.value)}
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
              className="flex-1 bg-[#120606] border-2 border-[#2a1a1a] text-[#666] hover:bg-[#2a1a1a] hover:text-white h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
              style={{ WebkitTapHighlightColor: "transparent" }}
              onClick={handleClear}
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

"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Search } from "lucide-react";

interface SearchFormProps {
  onSearch: (firstName: string, lastName: string) => void;
  isLoading?: boolean;
}

export function SearchForm({ onSearch, isLoading = false }: SearchFormProps) {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch(firstName, lastName);
  };

  return (
    <Card className="w-full max-w-md mx-auto bg-[#0a0303] border border-[#1a0a0a] shadow-[0_0_15px_rgba(0,0,0,0.5)] overflow-hidden">
      <CardContent className="p-5 space-y-4">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Input
              type="text"
              placeholder="Emri"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              className="w-full bg-[#120606] border border-[#1a0a0a] text-white placeholder:text-[#666] placeholder:font-normal focus-visible:ring-[#333] focus-visible:ring-offset-0 h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
              style={{ WebkitTapHighlightColor: "transparent" }}
            />
          </div>
          <div className="space-y-2">
            <Input
              type="text"
              placeholder="Mbiemri"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              className="w-full bg-[#120606] border border-[#1a0a0a] text-white placeholder:text-[#666] placeholder:font-normal focus-visible:ring-[#333] focus-visible:ring-offset-0 h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation"
              style={{ WebkitTapHighlightColor: "transparent" }}
            />
          </div>
          <div className="flex gap-3">
            <Button
              type="button"
              variant="outline"
              className="flex-1 bg-[#120606] text-[#999] border-[#1a0a0a] hover:bg-[#1a0a0a] hover:text-white h-12 shadow-[0_0_1px_1px_rgba(0,0,0,0.2),inset_0_0_0_1px_rgba(255,255,255,0.05)] relative overflow-hidden group touch-manipulation font-normal"
              style={{ WebkitTapHighlightColor: "transparent" }}
              onClick={() => {
                setFirstName("");
                setLastName("");
              }}
            >
              <span className="absolute inset-0 bg-gradient-to-br from-[#ffffff10] to-transparent opacity-0 group-hover:opacity-100 transition-opacity"></span>
              <span className="relative z-10">Pastro</span>
            </Button>
            <Button
              type="submit"
              className="flex-1 bg-gradient-to-br from-[#333] to-[#222] text-white border-[#444] hover:from-[#444] hover:to-[#333] h-12 font-bold shadow-[0_0_1px_1px_rgba(0,0,0,0.2)] relative overflow-hidden group touch-manipulation disabled:opacity-50 border"
              style={{ WebkitTapHighlightColor: "transparent" }}
              disabled={isLoading}
            >
              <span className="absolute inset-0 bg-gradient-to-br from-white to-transparent opacity-0 group-hover:opacity-10 transition-opacity"></span>
              <span className="relative z-10">
                {isLoading ? "Duke kërkuar..." : "Kërko"}
              </span>
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

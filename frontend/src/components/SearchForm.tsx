"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { cardStyles } from "./ui/card-styles";
import { useRouter, useSearchParams } from "next/navigation";

interface PhoneInputProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

function PhoneInput({ value, onChange, disabled }: PhoneInputProps) {
  const displayPhone = (phone: string) => {
    // Always start with "06"
    let display = "06";

    // Add the remaining digits with underscores for empty positions
    for (let i = 2; i < 10; i++) {
      if (i === 2) {
        display += phone[i] || "_";
      } else {
        display += " " + (phone[i] || "_");
      }
    }

    return display;
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = e.target.value;

    // Extract only digits from the input
    const digits = inputValue.replace(/\D/g, "");

    // Always ensure we start with "06" and add additional digits
    let newValue = "06";

    // Add digits after position 2, but skip if user typed 0 and 6 at the beginning
    if (digits.length > 2) {
      if (digits.startsWith("06")) {
        newValue = digits.slice(0, 10); // Take up to 10 digits
      } else {
        // If user typed something else, extract meaningful digits
        const meaningfulDigits = digits.replace(/^0?6?/, "");
        newValue = "06" + meaningfulDigits.slice(0, 8); // Max 8 additional digits
      }
    } else if (digits.length === 2 && digits !== "06") {
      // If user typed 2 digits but not "06", use the second digit
      newValue = "06" + digits.slice(1);
    } else if (digits.length === 1 && digits !== "0") {
      // If user typed 1 digit and it's not "0", add it after "06"
      newValue = "06" + digits;
    }

    onChange(newValue);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    const cursorPosition = e.currentTarget.selectionStart || 0;

    // Prevent deletion of the "06" prefix
    if ((e.key === "Backspace" || e.key === "Delete") && cursorPosition <= 2) {
      e.preventDefault();
    }

    // Handle backspace to remove the last digit
    if (e.key === "Backspace" && cursorPosition > 2) {
      e.preventDefault();
      const newValue = value.slice(0, -1);
      if (newValue.length >= 2) {
        onChange(newValue);
      }
    }
  };

  const handleClick = (e: React.MouseEvent<HTMLInputElement>) => {
    const target = e.target as HTMLInputElement;
    // Position cursor at the end of the entered digits
    const displayValue = displayPhone(value);
    let cursorPosition = value.length;

    // If clicking on underscore positions, move cursor to end of entered digits
    if (target.selectionStart !== null) {
      const clickedChar = displayValue[target.selectionStart];
      if (clickedChar === "_" || target.selectionStart < 2) {
        // Position cursor right after the last entered digit
        cursorPosition = Math.min(value.length, displayValue.length);
        target.setSelectionRange(cursorPosition, cursorPosition);
      }
    }
  };

  const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
    const target = e.target as HTMLInputElement;
    // Position cursor at the end of the entered digits when focused
    const cursorPosition = value.length;
    setTimeout(() => {
      target.setSelectionRange(cursorPosition, cursorPosition);
    }, 0);
  };

  return (
    <div className="relative">
      <Input
        type="tel"
        placeholder="06_ _ _ _ _ _ _ _"
        value={displayPhone(value)}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        onClick={handleClick}
        onFocus={handleFocus}
        className="w-full bg-[#120606] border-2 border-[#2a1a1a] text-white placeholder:text-[#666] placeholder:font-normal focus-visible:ring-[#333] focus-visible:ring-offset-0 h-12 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2)] touch-manipulation font-mono tracking-wider"
        style={{ WebkitTapHighlightColor: "transparent" }}
        disabled={disabled}
        maxLength={19} // Account for spaces and underscores
      />
    </div>
  );
}

interface SearchFormProps {
  onSearch: (emri: string, mbiemri: string) => void;
  onSearchTarga: (numriTarges: string) => void;
  onSearchTelefon: (numriTelefonit: string) => void;
  onClear: () => void;
  isLoading: boolean;
  defaultValues?: { emri: string; mbiemri: string } | null;
}

export function SearchForm({
  onSearch,
  onSearchTarga,
  onSearchTelefon,
  onClear,
  isLoading,
  defaultValues,
}: SearchFormProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [activeTab, setActiveTab] = useState<"name" | "targa" | "telefon">(
    "name"
  );
  const [emri, setEmri] = useState("");
  const [mbiemri, setMbiemri] = useState("");
  const [targa, setTarga] = useState("");
  const [telefon, setTelefon] = useState("06");

  useEffect(() => {
    const urlTarga = searchParams.get("targa");
    const urlTelefon = searchParams.get("telefon");
    if (urlTarga) {
      setActiveTab("targa");
      setTarga(urlTarga);
    } else if (urlTelefon) {
      setActiveTab("telefon");
      setTelefon(urlTelefon.startsWith("06") ? urlTelefon : "06" + urlTelefon);
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
      params.delete("telefon");
      router.push(`?${params.toString()}`, { scroll: false });

      onSearch(emri, mbiemri);
    } else if (activeTab === "targa") {
      const params = new URLSearchParams(searchParams.toString());
      params.set("targa", targa);
      params.delete("emri");
      params.delete("mbiemri");
      params.delete("telefon");
      router.push(`?${params.toString()}`, { scroll: false });

      onSearchTarga(targa);
    } else if (activeTab === "telefon") {
      if (telefon.length < 10) {
        return; // Don't submit if phone number is incomplete
      }

      const params = new URLSearchParams(searchParams.toString());
      params.set("telefon", telefon);
      params.delete("emri");
      params.delete("mbiemri");
      params.delete("targa");
      router.push(`?${params.toString()}`, { scroll: false });

      onSearchTelefon(telefon);
    }
  };

  const handleClear = () => {
    setEmri("");
    setMbiemri("");
    setTarga("");
    setTelefon("06");
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
            <button
              type="button"
              onClick={() => {
                setActiveTab("telefon");
                if (telefon === "") {
                  setTelefon("06");
                }
              }}
              className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                activeTab === "telefon"
                  ? "bg-[#2a1a1a] text-white shadow-sm"
                  : "text-[#999] hover:text-white hover:bg-[#1a1a1a]"
              }`}
            >
              Telefon
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
          ) : activeTab === "targa" ? (
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
          ) : (
            <div className="space-y-2">
              <PhoneInput
                value={telefon}
                onChange={setTelefon}
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
              disabled={
                isLoading || (activeTab === "telefon" && telefon.length < 10)
              }
            >
              {isLoading ? "Duke kërkuar..." : "Kërko"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { PhoneInput } from "./PhoneInput";
import { TargaInput } from "./TargaInput";
import { cardStyles } from "./ui/card-styles";

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
      if (targa.length < 7) {
        return;
      }

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
          <div className="flex space-x-1 p-1 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary">
            <button
              type="button"
              onClick={() => setActiveTab("name")}
              className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                activeTab === "name"
                  ? "bg-surface-tertiary text-text-primary"
                  : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
              }`}
            >
              Emri
            </button>
            <button
              type="button"
              onClick={() => setActiveTab("targa")}
              className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                activeTab === "targa"
                  ? "bg-surface-tertiary text-text-primary"
                  : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
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
                  ? "bg-surface-tertiary text-text-primary"
                  : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
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
                className="w-full bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary placeholder:text-text-tertiary placeholder:font-normal focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-12 touch-manipulation"
                style={{ WebkitTapHighlightColor: "transparent" }}
                disabled={isLoading}
              />
              <Input
                type="text"
                placeholder="Mbiemri"
                value={mbiemri}
                onChange={(e) => setMbiemri(e.target.value)}
                className="w-full bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary placeholder:text-text-tertiary placeholder:font-normal focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-12 touch-manipulation"
                style={{ WebkitTapHighlightColor: "transparent" }}
                disabled={isLoading}
              />
            </div>
          ) : activeTab === "targa" ? (
            <div className="">
              <TargaInput
                value={targa}
                onChange={setTarga}
                disabled={isLoading}
              />
            </div>
          ) : (
            <div className="">
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
              className="flex-1 bg-surface-secondary border-2 border-border-semantic-secondary text-text-tertiary hover:bg-surface-tertiary hover:text-text-primary h-12 touch-manipulation"
              style={{ WebkitTapHighlightColor: "transparent" }}
              onClick={handleClear}
            >
              Pastro
            </Button>
            <Button
              type="submit"
              className="flex-1 bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary hover:bg-surface-tertiary h-12 touch-manipulation disabled:opacity-50"
              style={{ WebkitTapHighlightColor: "transparent" }}
              disabled={
                isLoading ||
                (activeTab === "telefon" && telefon.length < 10) ||
                (activeTab === "targa" && targa.length < 7)
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

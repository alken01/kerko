"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useTranslation } from "@/i18n/TranslationContext";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useRef, useState } from "react";
import { PhoneInput } from "./PhoneInput";
import { TargaInput } from "./TargaInput";
import { cardStyles } from "./ui/card-styles";
import { PHONE_NUMBER_LENGTH, LICENSE_PLATE_LENGTH, ALBANIAN_PHONE_PREFIX } from "@/lib/constants";
import {
  saveSearchToHistory,
  getSearchHistory,
  clearSearchHistory,
  SearchHistoryItem,
} from "@/services/storage";

interface SearchFormProps {
  onClear: () => void;
  isLoading: boolean;
  defaultValues?: { emri: string; mbiemri: string } | null;
}

export function SearchForm({
  onClear,
  isLoading,
  defaultValues,
}: SearchFormProps) {
  const { t } = useTranslation();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [activeTab, setActiveTab] = useState<"name" | "targa" | "telefon" | "np">(
    "name"
  );
  const [emri, setEmri] = useState("");
  const [mbiemri, setMbiemri] = useState("");
  const mbiemriRef = useRef<HTMLInputElement>(null);
  const [targa, setTarga] = useState("");
  const [telefon, setTelefon] = useState(ALBANIAN_PHONE_PREFIX);
  const [numriPersonal, setNumriPersonal] = useState("");
  const [history, setHistory] = useState<SearchHistoryItem[]>([]);
  const [showHistory, setShowHistory] = useState(false);
  const formRef = useRef<HTMLDivElement>(null);

  const loadHistory = useCallback(async () => {
    try {
      const items = await getSearchHistory();
      setHistory(items);
    } catch {
      // IndexedDB unavailable
    }
  }, []);

  useEffect(() => {
    loadHistory();
  }, [loadHistory]);

  // Close history dropdown when clicking outside the form
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (formRef.current && !formRef.current.contains(e.target as Node)) {
        setShowHistory(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    const urlTarga = searchParams.get("targa");
    const urlTelefon = searchParams.get("telefon");
    const urlNp = searchParams.get("np");
    if (urlTarga) {
      setActiveTab("targa");
      setTarga(urlTarga);
    } else if (urlTelefon) {
      setActiveTab("telefon");
      setTelefon(urlTelefon.startsWith(ALBANIAN_PHONE_PREFIX) ? urlTelefon : ALBANIAN_PHONE_PREFIX + urlTelefon);
    } else if (urlNp) {
      setActiveTab("np");
      setNumriPersonal(urlNp);
    } else if (defaultValues) {
      setActiveTab("name");
      setEmri(defaultValues.emri);
      setMbiemri(defaultValues.mbiemri);
    }
  }, [defaultValues, searchParams]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setShowHistory(false);
    if (activeTab === "name") {
      const params = new URLSearchParams(searchParams.toString());
      params.set("emri", emri);
      params.set("mbiemri", mbiemri);
      params.delete("targa");
      params.delete("telefon");
      params.delete("np");
      router.push(`?${params.toString()}`, { scroll: false });
      saveSearchToHistory("person", { emri, mbiemri }).then(loadHistory);
    } else if (activeTab === "targa") {
      if (targa.length < LICENSE_PLATE_LENGTH) {
        return;
      }

      const params = new URLSearchParams(searchParams.toString());
      params.set("targa", targa);
      params.delete("emri");
      params.delete("mbiemri");
      params.delete("telefon");
      params.delete("np");
      router.push(`?${params.toString()}`, { scroll: false });
      saveSearchToHistory("targa", { targa }).then(loadHistory);
    } else if (activeTab === "telefon") {
      if (telefon.length < PHONE_NUMBER_LENGTH) {
        return;
      }

      const params = new URLSearchParams(searchParams.toString());
      params.set("telefon", telefon);
      params.delete("emri");
      params.delete("mbiemri");
      params.delete("targa");
      params.delete("np");
      router.push(`?${params.toString()}`, { scroll: false });
      saveSearchToHistory("telefon", { telefon }).then(loadHistory);
    } else if (activeTab === "np") {
      if (numriPersonal.length < 2) {
        return;
      }

      const params = new URLSearchParams(searchParams.toString());
      params.set("np", numriPersonal);
      params.delete("emri");
      params.delete("mbiemri");
      params.delete("targa");
      params.delete("telefon");
      router.push(`?${params.toString()}`, { scroll: false });
      saveSearchToHistory("np", { np: numriPersonal }).then(loadHistory);
    }
  };

  const handleClear = () => {
    setEmri("");
    setMbiemri("");
    setTarga("");
    setTelefon(ALBANIAN_PHONE_PREFIX);
    setNumriPersonal("");
    router.replace("/", { scroll: false });
    if (onClear) {
      onClear();
    }
  };

  const handleSelectHistory = (item: SearchHistoryItem) => {
    setShowHistory(false);
    const tabMap: Record<SearchHistoryItem["type"], "name" | "targa" | "telefon" | "np"> = {
      person: "name",
      targa: "targa",
      telefon: "telefon",
      np: "np",
    };
    setActiveTab(tabMap[item.type]);

    if (item.type === "person") {
      setEmri(item.terms.emri || "");
      setMbiemri(item.terms.mbiemri || "");
    } else if (item.type === "targa") {
      setTarga(item.terms.targa || "");
    } else if (item.type === "telefon") {
      setTelefon(item.terms.telefon || ALBANIAN_PHONE_PREFIX);
    } else if (item.type === "np") {
      setNumriPersonal(item.terms.np || "");
    }

    // Build URL params and navigate
    const params = new URLSearchParams();
    if (item.type === "person") {
      params.set("emri", item.terms.emri || "");
      params.set("mbiemri", item.terms.mbiemri || "");
    } else if (item.type === "targa") {
      params.set("targa", item.terms.targa || "");
    } else if (item.type === "telefon") {
      params.set("telefon", item.terms.telefon || "");
    } else if (item.type === "np") {
      params.set("np", item.terms.np || "");
    }
    router.push(`?${params.toString()}`, { scroll: false });
  };

  const handleClearHistory = async () => {
    await clearSearchHistory();
    setHistory([]);
    setShowHistory(false);
  };

  const formatHistoryLabel = (item: SearchHistoryItem): string => {
    if (item.type === "person") {
      return [item.terms.emri, item.terms.mbiemri].filter(Boolean).join(" ");
    }
    if (item.type === "targa") return item.terms.targa || "";
    if (item.type === "telefon") return item.terms.telefon || "";
    if (item.type === "np") return item.terms.np || "";
    return "";
  };

  const typeIcon: Record<SearchHistoryItem["type"], string> = {
    person: t("search.nameTab"),
    targa: t("search.plateTab"),
    telefon: t("search.phoneTab"),
    np: t("search.personalNumberTab"),
  };

  return (
    <div ref={formRef} className="relative">
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
                {t("search.nameTab")}
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
                {t("search.plateTab")}
              </button>
              <button
                type="button"
                onClick={() => {
                  setActiveTab("telefon");
                  if (telefon === "") {
                    setTelefon(ALBANIAN_PHONE_PREFIX);
                  }
                }}
                className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                  activeTab === "telefon"
                    ? "bg-surface-tertiary text-text-primary"
                    : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
                }`}
              >
                {t("search.phoneTab")}
              </button>
              <button
                type="button"
                onClick={() => setActiveTab("np")}
                className={`flex-1 px-3 py-2 text-xs sm:text-sm font-medium rounded-md transition-all duration-200 ${
                  activeTab === "np"
                    ? "bg-surface-tertiary text-text-primary"
                    : "text-text-tertiary hover:text-text-primary hover:bg-surface-interactive"
                }`}
              >
                {t("search.personalNumberTab")}
              </button>
            </div>

            {activeTab === "name" ? (
              <div className="space-y-2">
                <Input
                  type="text"
                  placeholder={t("search.firstName")}
                  value={emri}
                  onChange={(e) => setEmri(e.target.value)}
                  onFocus={() => history.length > 0 && setShowHistory(true)}
                  onInput={(e) => {
                    // Detect iOS autocomplete (fills multiple chars at once) and move to mbiemri
                    const input = e.target as HTMLInputElement;
                    if (input.value.length > 1 && emri.length === 0) {
                      setTimeout(() => mbiemriRef.current?.focus(), 0);
                    }
                  }}
                  autoComplete="given-name"
                  className="w-full bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary placeholder:text-text-tertiary placeholder:font-normal focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-12 touch-manipulation"
                  disabled={isLoading}
                />
                <Input
                  ref={mbiemriRef}
                  type="text"
                  placeholder={t("search.lastName")}
                  value={mbiemri}
                  onChange={(e) => setMbiemri(e.target.value)}
                  onFocus={() => history.length > 0 && setShowHistory(true)}
                  autoComplete="family-name"
                  className="w-full bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary placeholder:text-text-tertiary placeholder:font-normal focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-12 touch-manipulation"
                  disabled={isLoading}
                />
              </div>
            ) : activeTab === "targa" ? (
              <div className="" onFocus={() => history.length > 0 && setShowHistory(true)}>
                <TargaInput
                  value={targa}
                  onChange={setTarga}
                  disabled={isLoading}
                />
              </div>
            ) : activeTab === "telefon" ? (
              <div className="" onFocus={() => history.length > 0 && setShowHistory(true)}>
                <PhoneInput
                  value={telefon}
                  onChange={setTelefon}
                  disabled={isLoading}
                />
              </div>
            ) : (
              <div className="">
                <Input
                  type="text"
                  placeholder={t("search.personalNumber")}
                  value={numriPersonal}
                  onChange={(e) => setNumriPersonal(e.target.value.toUpperCase())}
                  onFocus={() => history.length > 0 && setShowHistory(true)}
                  className="w-full bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary placeholder:text-text-tertiary placeholder:font-normal focus-visible:ring-border-semantic-interactive focus-visible:ring-offset-0 h-12 touch-manipulation tracking-widest font-mono"
                  disabled={isLoading}
                />
              </div>
            )}

            <div className="flex gap-3">
              <Button
                type="button"
                variant="outline"
                className="flex-1 bg-surface-secondary border-2 border-border-semantic-secondary text-text-tertiary hover:bg-surface-tertiary hover:text-text-primary h-12 touch-manipulation"
                onClick={handleClear}
              >
                {t("search.clear")}
              </Button>
              <Button
                type="submit"
                className="flex-1 bg-surface-secondary border-2 border-border-semantic-secondary text-text-primary hover:bg-surface-tertiary h-12 touch-manipulation disabled:opacity-50"
                disabled={
                  isLoading ||
                  (activeTab === "telefon" && telefon.length < PHONE_NUMBER_LENGTH) ||
                  (activeTab === "targa" && targa.length < LICENSE_PLATE_LENGTH) ||
                  (activeTab === "np" && numriPersonal.length < 2)
                }
              >
                {isLoading ? t("search.searching") : t("search.submit")}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {showHistory && history.length > 0 && (
        <div className="absolute left-0 right-0 z-50 mt-1 rounded-lg border-2 border-border-semantic-secondary bg-surface-primary shadow-lg overflow-hidden">
          <div className="flex items-center justify-between px-3 py-2 border-b border-border-semantic-secondary">
            <span className="text-xs font-medium text-text-tertiary">
              {t("search.recentSearches")}
            </span>
            <button
              type="button"
              onClick={handleClearHistory}
              className="text-xs text-text-tertiary hover:text-text-primary transition-colors"
            >
              {t("search.clearHistory")}
            </button>
          </div>
          <ul className="max-h-60 overflow-y-auto">
            {history.map((item) => (
              <li key={item.id}>
                <button
                  type="button"
                  onClick={() => handleSelectHistory(item)}
                  className="w-full text-left px-3 py-2.5 hover:bg-surface-secondary transition-colors flex items-center gap-2"
                >
                  <span className="text-[10px] font-medium text-text-tertiary bg-surface-secondary rounded px-1.5 py-0.5 shrink-0">
                    {typeIcon[item.type]}
                  </span>
                  <span className="text-sm text-text-primary truncate">
                    {formatHistoryLabel(item)}
                  </span>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

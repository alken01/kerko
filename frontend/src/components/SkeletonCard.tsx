"use client";

import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { cardStyles } from "@/components/ui/card-styles";
import { useTranslation } from "@/i18n/TranslationContext";

export function SkeletonCard() {
  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        {/* Name title */}
        <Skeleton className="h-6 w-40 mb-3" />
        {/* Three info rows */}
        <div className="space-y-2 mt-1.5">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-3/4" />
          <Skeleton className="h-4 w-2/3" />
        </div>
      </CardHeader>
      <CardContent className={cardStyles.content}>
        <div className="px-5 py-3">
          <Skeleton className="h-5 w-36" />
        </div>
      </CardContent>
    </Card>
  );
}

export function SkeletonGrid({
  count = 4,
  isTargaSearch = false,
  isTelefonSearch = false,
}: {
  count?: number;
  isTargaSearch?: boolean;
  isTelefonSearch?: boolean;
}) {
  const { t } = useTranslation();

  const tabs = isTargaSearch
    ? [t("results.plates")]
    : isTelefonSearch
    ? [t("results.patrons")]
    : [t("results.people"), t("results.salaries"), t("results.plates"), t("results.patrons")];

  return (
    <div className="flex flex-col gap-4 max-w-4xl mx-auto w-full">
      {/* Tabs with real labels, skeleton count badge */}
      <div className="flex space-x-1 p-1 bg-surface-secondary rounded-lg border-2 border-border-semantic-secondary overflow-x-auto">
        {tabs.map((label, i) => (
          <div
            key={i}
            className={`flex-1 flex-shrink-0 min-w-fit px-3 py-2 text-xs font-medium rounded-md flex items-center justify-center gap-1 ${
              i === 0
                ? "bg-surface-tertiary text-text-primary"
                : "text-text-tertiary"
            }`}
          >
            <span>{label}</span>
            <Skeleton className="w-6 h-4 rounded-full" />
          </div>
        ))}
      </div>
      {/* Skeleton cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
        {Array.from({ length: count }).map((_, i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
    </div>
  );
}

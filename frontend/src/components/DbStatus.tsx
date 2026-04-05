"use client";

import { useEffect, useState } from "react";
import { ApiService } from "@/services/api";
import { useTranslation } from "@/i18n/TranslationContext";
import { Database } from "lucide-react";

export function DbStatus() {
  const { t } = useTranslation();
  const [totalRecords, setTotalRecords] = useState<number | null>(null);

  useEffect(() => {
    ApiService.getDbStatus()
      .then((status) => {
        const total = status.reduce((sum, table) => {
          return sum + Object.values(table).reduce((s, v) => s + v, 0);
        }, 0);
        setTotalRecords(total);
      })
      .catch(() => {});
  }, []);

  if (totalRecords === null) return null;

  return (
    <p className="text-center text-xs text-text-tertiary flex items-center justify-center gap-1">
      <Database className="h-3 w-3" />
      {t("app.searchingRecords").replace("{count}", new Intl.NumberFormat().format(totalRecords))}
    </p>
  );
}

import { RrogatResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Wallet, Briefcase, CircleDot, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { useTranslation } from "@/i18n/TranslationContext";
import { useState } from "react";

interface RrogatCardProps {
  rrogat: RrogatResponse;
  defaultExpanded?: boolean;
}

export function RrogatCard({ rrogat, defaultExpanded }: RrogatCardProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(defaultExpanded ?? false);

  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className="absolute top-3 right-3 z-20">
          <SaveButton type="rrogat" data={rrogat} />
        </div>
        <div className="relative z-10">
          <h2 className={cardStyles.title}>
            {rrogat.emri} <span className="font-bold">{rrogat.mbiemri}</span>
          </h2>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={CircleDot}
              label={t("salary.personalNumber")}
              value={rrogat.numriPersonal || "N/A"}
            />
            <InfoItem
              icon={Briefcase}
              label={t("salary.profession")}
              value={rrogat.profesioni || "N/A"}
            />
          </div>
        </div>
      </CardHeader>

      <CardContent className={cardStyles.content}>
        <div className={cardStyles.section}>
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="w-full flex items-center justify-between group hover:opacity-80 transition-opacity"
          >
            <h3 className={cardStyles.sectionTitle}>
              <Wallet className={cardStyles.sectionIcon} />
              {t("salary.financialInfo")}
            </h3>
            <ChevronDown className={cn(
              "h-5 w-5 text-text-tertiary transition-transform duration-300 ease-[cubic-bezier(0.4,0,0.2,1)]",
              isExpanded && "rotate-180"
            )} />
          </button>
          <div className={cn(
            "grid transition-[grid-template-rows] duration-300 ease-[cubic-bezier(0.4,0,0.2,1)]",
            isExpanded ? "grid-rows-[1fr]" : "grid-rows-[0fr]"
          )}>
            <div className="overflow-hidden">
              <div className={cn(cardStyles.detailsContainer, "mt-2")}>
                <div className={cardStyles.detailsGrid}>
                  <DetailRow label={t("salary.nipt")} value={rrogat.nipt || "N/A"} />
                  <DetailRow label={t("salary.drt")} value={rrogat.drt || "N/A"} />
                  <DetailRow
                    label={t("salary.grossSalary")}
                    value={
                      rrogat.pagaBruto ? (
                        <div className="text-lg font-bold text-text-primary">
                          {new Intl.NumberFormat("sq-AL").format(rrogat.pagaBruto)}
                          <span className="text-text-secondary ml-1">ALL</span>
                        </div>
                      ) : (
                        "N/A"
                      )
                    }
                  />
                  <DetailRow label={t("salary.category")} value={rrogat.kategoria || "N/A"} />
                </div>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

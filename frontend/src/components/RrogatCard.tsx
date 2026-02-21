import { RrogatResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Wallet, Briefcase, CircleDot, ChevronDown, ChevronUp } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { useTranslation } from "@/i18n/TranslationContext";
import { useState } from "react";

interface RrogatCardProps {
  rrogat: RrogatResponse;
}

export function RrogatCard({ rrogat }: RrogatCardProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(false);

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
            {isExpanded ? (
              <ChevronUp className="h-5 w-5 text-gray-400 group-hover:text-gray-600 transition-colors" />
            ) : (
              <ChevronDown className="h-5 w-5 text-gray-400 group-hover:text-gray-600 transition-colors" />
            )}
          </button>
          {isExpanded && (
            <div className={cardStyles.detailsContainer}>
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
          )}
        </div>
      </CardContent>
    </Card>
  );
}

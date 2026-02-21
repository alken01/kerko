import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Car, ChevronDown, CircleDot } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "./ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { useTranslation } from "@/i18n/TranslationContext";
import { TargatResponse } from "@/types/kerko";
import { cn } from "@/lib/utils";
import { useState } from "react";
import Link from "next/link";

interface TargatCardProps {
  targat: TargatResponse;
  defaultExpanded?: boolean;
}

export function TargatCard({ targat, defaultExpanded }: TargatCardProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(defaultExpanded ?? false);

  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className="absolute top-3 right-3 z-20">
          <SaveButton type="targat" data={targat} />
        </div>
        <div className="relative z-10">
          <Link
            href={`/?emri=${encodeURIComponent(targat.emri || "")}&mbiemri=${encodeURIComponent(targat.mbiemri || "")}`}
            className={cn(cardStyles.title, "underline decoration-dotted underline-offset-2 hover:text-blue-600 transition-colors")}
          >
            {targat.emri} <span className="font-bold">{targat.mbiemri}</span>
          </Link>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={CircleDot}
              label={t("plate.personalNumber")}
              value={targat.numriPersonal || "N/A"}
            />
            <InfoItem
              icon={Car}
              label={t("plate.licensePlate")}
              value={targat.numriTarges || "N/A"}
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
              <Car className={cardStyles.sectionIcon} />
              {t("plate.vehicleInfo")}
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
                  <DetailRow label={t("plate.brand")} value={targat.marka || "N/A"} />
                  <DetailRow label={t("plate.model")} value={targat.modeli || "N/A"} />
                  <DetailRow label={t("plate.color")} value={targat.ngjyra || "N/A"} />
                </div>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

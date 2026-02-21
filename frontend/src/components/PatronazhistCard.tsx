import { PatronazhistResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  MapPin,
  CircleDot,
  Calendar,
  User,
  ChevronDown,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { useTranslation } from "@/i18n/TranslationContext";
import Link from "next/link";
import { useState } from "react";

interface PatronazhistCardProps {
  patronazhist: PatronazhistResponse;
  defaultExpanded?: boolean;
}

export function PatronazhistCard({ patronazhist, defaultExpanded }: PatronazhistCardProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(defaultExpanded ?? false);

  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className="absolute top-3 right-3 z-20">
          <SaveButton type="patronazhist" data={patronazhist} />
        </div>
        <div className="relative z-10">
          <h2 className={cardStyles.title}>
            {patronazhist.emri}{" "}
            <span className="font-bold">{patronazhist.mbiemri}</span>
          </h2>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={CircleDot}
              label={t("patron.personalNumber")}
              value={patronazhist.numriPersonal || "N/A"}
            />
            <InfoItem
              icon={MapPin}
              label={t("patron.placeOfBirth")}
              value={patronazhist.vendlindja || "N/A"}
            />
            <InfoItem
              icon={Calendar}
              label={t("patron.born")}
              value={patronazhist.datelindja}
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
              <User className={cardStyles.sectionIcon} />
              {t("patron.personalInfo")}
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
                <DetailRow
                  label={t("patron.fathersName")}
                  value={
                    patronazhist.atesi ? (
                      <Link
                        href={`/?emri=${encodeURIComponent(patronazhist.atesi)}&mbiemri=${encodeURIComponent(patronazhist.mbiemri || "")}`}
                        className="text-text-primary font-bold uppercase text-right flex-1 underline decoration-dotted underline-offset-2 hover:text-blue-600 transition-colors"
                      >
                        {patronazhist.atesi}
                      </Link>
                    ) : (
                      "N/A"
                    )
                  }
                />
                <DetailRow
                  label={t("patron.residenceCode")}
                  value={patronazhist.kodBanese || "N/A"}
                />
                <DetailRow label={t("patron.phone")} value={patronazhist.tel || "N/A"} />
                <DetailRow
                  label={t("patron.preference")}
                  value={patronazhist.preferenca || "N/A"}
                />
                <DetailRow
                  label={t("patron.securePreference")}
                  value={patronazhist.iSigurte || "N/A"}
                />
                <DetailRow
                  label={t("patron.census2013Preference")}
                  value={patronazhist.census2013Preferenca || "N/A"}
                />
                <DetailRow
                  label={t("patron.census2013Security")}
                  value={
                    patronazhist.census2013Siguria
                      ? `${patronazhist.census2013Siguria}/10`
                      : "N/A"
                  }
                />
                {patronazhist.koment && (
                  <DetailRow
                    label={t("patron.comment")}
                    value={patronazhist.koment || "N/A"}
                  />
                )}
                <DetailRow
                  label={t("patron.patronazhist")}
                  value={patronazhist.patronazhisti || "N/A"}
                />
                <DetailRow label={t("patron.qv")} value={patronazhist.qv || "N/A"} />
                <DetailRow
                  label={t("patron.listNumber")}
                  value={patronazhist.listaNr || "N/A"}
                />
                <DetailRow
                  label={t("patron.company")}
                  value={patronazhist.kompania || "N/A"}
                />
                {patronazhist.emigrant && (
                  <>
                    <DetailRow
                      label={t("patron.emigrant")}
                      value={patronazhist.emigrant || "N/A"}
                    />
                    {patronazhist.country && (
                      <DetailRow
                        label={t("patron.emigrationCountry")}
                        value={patronazhist.country || "N/A"}
                      />
                    )}
                  </>
                )}
              </div>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

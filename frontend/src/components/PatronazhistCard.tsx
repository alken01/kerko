import { PatronazhistResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  MapPin,
  CircleDot,
  Calendar,
  User,
  ChevronDown,
  ChevronUp,
} from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { useTranslation } from "@/i18n/TranslationContext";
import Link from "next/link";
import { useState } from "react";

interface PatronazhistCardProps {
  patronazhist: PatronazhistResponse;
}

export function PatronazhistCard({ patronazhist }: PatronazhistCardProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(false);

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
            {isExpanded ? (
              <ChevronUp className="h-5 w-5 text-gray-400 group-hover:text-gray-600 transition-colors" />
            ) : (
              <ChevronDown className="h-5 w-5 text-gray-400 group-hover:text-gray-600 transition-colors" />
            )}
          </button>
          {isExpanded && (
            <div className={cardStyles.detailsContainer}>
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
          )}
        </div>
      </CardContent>
    </Card>
  );
}

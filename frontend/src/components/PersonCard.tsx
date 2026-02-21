import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { cardStyles, DetailRow, InfoItem } from "@/components/ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { useTranslation } from "@/i18n/TranslationContext";
import { PersonResponse } from "@/types/kerko";
import { Calendar, ChevronDown, Home, MapPin, User } from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { cn } from "@/lib/utils";

interface PersonCardProps {
  person: PersonResponse;
  defaultExpanded?: boolean;
}

export function PersonCard({ person, defaultExpanded }: PersonCardProps) {
  const { t } = useTranslation();
  const [isExpanded, setIsExpanded] = useState(defaultExpanded ?? false);
  const isMarriedWoman = person.seksi === "F" && person.gjendjeCivile?.toLowerCase().includes("martuar");
  const parentLinkHint = isMarriedWoman ? "&hint=mbiemri" : "";

  const formatDate = (dateString: string | null) => {
    if (!dateString) return "N/A";
    try {
      // Parse DD/MM/YY format
      const [month, day, year] = dateString.split("/");
      return `${day}/${month}/${year}`;
    } catch {
      return "N/A";
    }
  };

  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className="absolute top-3 right-3 z-20">
          <SaveButton type="person" data={person} />
        </div>
        <div className="relative z-10">
          <h2 className={cardStyles.title}>
            {person.emri} <span className="font-bold">{person.mbiemri}</span>
          </h2>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={Calendar}
              label={t("person.born")}
              value={formatDate(person.datelindja)}
            />
            <InfoItem
              icon={MapPin}
              label={t("person.placeOfBirth")}
              value={person.vendlindja || "N/A"}
            />
            <InfoItem
              icon={Home}
              label={t("person.city")}
              value={person.qyteti || "N/A"}
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
              {t("person.personalInfo")}
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
                    label={t("person.address")}
                    value={
                      person.adresa && person.nrBaneses
                        ? `${person.adresa} ${person.nrBaneses}`
                        : person.adresa || person.nrBaneses || "N/A"
                    }
                  />
                  <DetailRow
                    label={t("person.fathersName")}
                    value={
                      person.atesi ? (
                        <Link
                          href={`/?emri=${encodeURIComponent(person.atesi)}&mbiemri=${encodeURIComponent(person.mbiemri || "")}${parentLinkHint}`}
                          className="text-text-primary font-bold uppercase text-right flex-1 underline decoration-dotted underline-offset-2 hover:text-blue-600 transition-colors"
                        >
                          {person.atesi}
                        </Link>
                      ) : (
                        "N/A"
                      )
                    }
                  />
                  <DetailRow
                    label={t("person.mothersName")}
                    value={
                      person.amesi ? (
                        <Link
                          href={`/?emri=${encodeURIComponent(person.amesi)}&mbiemri=${encodeURIComponent(person.mbiemri || "")}${parentLinkHint}`}
                          className="text-text-primary font-bold uppercase text-right flex-1 underline decoration-dotted underline-offset-2 hover:text-blue-600 transition-colors"
                        >
                          {person.amesi}
                        </Link>
                      ) : (
                        "N/A"
                      )
                    }
                  />
                  <DetailRow label={t("person.nationality")} value={person.kombesia || "N/A"} />
                  <DetailRow
                    label={t("person.gender")}
                    value={person.seksi === "F" ? t("person.female") : t("person.male")}
                  />
                  <DetailRow
                    label={t("person.maritalStatus")}
                    value={person.gjendjeCivile || "N/A"}
                  />
                  {person.lidhjaMeKryefamiljarin && (
                    <DetailRow
                      label={t("person.headOfHousehold")}
                      value={person.lidhjaMeKryefamiljarin}
                    />
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

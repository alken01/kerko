import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { cardStyles, DetailRow, InfoItem } from "@/components/ui/card-styles";
import { SaveButton } from "@/components/ui/save-button";
import { PersonResponse } from "@/types/kerko";
import { Calendar, ChevronDown, ChevronUp, Home, MapPin, User } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

interface PersonCardProps {
  person: PersonResponse;
}

export function PersonCard({ person }: PersonCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);

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
              label="Lindur"
              value={formatDate(person.datelindja)}
            />
            <InfoItem
              icon={MapPin}
              label="Vendlindja"
              value={person.vendlindja || "N/A"}
            />
            <InfoItem
              icon={Home}
              label="Qyteti"
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
              Informacion Personal
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
                  label="Adresa"
                  value={
                    person.adresa && person.nrBaneses
                      ? `${person.adresa} ${person.nrBaneses}`
                      : person.adresa || person.nrBaneses || "N/A"
                  }
                />
                <DetailRow
                  label="Emri i Babait"
                  value={
                    person.atesi ? (
                      <Link
                        href={`/?emri=${encodeURIComponent(person.atesi)}&mbiemri=${encodeURIComponent(person.mbiemri || "")}`}
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
                  label="Emri i Nënës"
                  value={
                    person.amesi ? (
                      <Link
                        href={`/?emri=${encodeURIComponent(person.amesi)}&mbiemri=${encodeURIComponent(person.mbiemri || "")}`}
                        className="text-text-primary font-bold uppercase text-right flex-1 underline decoration-dotted underline-offset-2 hover:text-blue-600 transition-colors"
                      >
                        {person.amesi}
                      </Link>
                    ) : (
                      "N/A"
                    )
                  }
                />
                <DetailRow label="Kombësia" value={person.kombesia || "N/A"} />
                <DetailRow
                  label="Gjinia"
                  value={person.seksi === "F" ? "Femër" : "Mashkull"}
                />
                <DetailRow
                  label="Gjendja Martesore"
                  value={person.gjendjeCivile || "N/A"}
                />
                {person.lidhjaMeKryefamiljarin && (
                  <DetailRow
                    label="Lidhja me Kryefamiljarin"
                    value={person.lidhjaMeKryefamiljarin}
                  />
                )}
              </div>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

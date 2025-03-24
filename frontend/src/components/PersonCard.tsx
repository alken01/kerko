import { PersonResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Calendar, MapPin, User, Home } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";

interface PersonCardProps {
  person: PersonResponse;
}

export function PersonCard({ person }: PersonCardProps) {
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
          <h3 className={cardStyles.sectionTitle}>
            <User className={cardStyles.sectionIcon} />
            Informacion Personal
          </h3>
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
              <DetailRow label="Emri i Babait" value={person.atesi || "N/A"} />
              <DetailRow label="Emri i Nënës" value={person.amesi || "N/A"} />
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
        </div>
      </CardContent>
    </Card>
  );
}

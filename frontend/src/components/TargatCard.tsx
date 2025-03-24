import { TargatResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Car, CircleDot } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";

interface TargatCardProps {
  targat: TargatResponse;
}

export function TargatCard({ targat }: TargatCardProps) {
  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className={cardStyles.headerGradient} />
        <div className="relative z-10">
          <h2 className={cardStyles.title}>
            {targat.emri} <span className="font-bold">{targat.mbiemri}</span>
          </h2>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={CircleDot}
              label="Numri Personal"
              value={targat.numriPersonal || "N/A"}
            />
            <InfoItem
              icon={Car}
              label="Targa"
              value={targat.numriTarges || "N/A"}
            />
          </div>
        </div>
      </CardHeader>

      <CardContent className={cardStyles.content}>
        <div className={cardStyles.section}>
          <h3 className={cardStyles.sectionTitle}>
            <Car className={cardStyles.sectionIcon} />
            Informacion i Automjetit
          </h3>
          <div className={cardStyles.detailsContainer}>
            <div className={cardStyles.detailsGrid}>
              <DetailRow label="Marka" value={targat.marka || "N/A"} />
              <DetailRow label="Modeli" value={targat.modeli || "N/A"} />
              <DetailRow label="Ngjyra" value={targat.ngjyra || "N/A"} />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

import { RrogatResponse } from "@/types/kerko";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Wallet, Briefcase, CircleDot } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "@/components/ui/card-styles";

interface RrogatCardProps {
  rrogat: RrogatResponse;
}

export function RrogatCard({ rrogat }: RrogatCardProps) {
  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className="relative z-10">
          <h2 className={cardStyles.title}>
            {rrogat.emri} <span className="font-bold">{rrogat.mbiemri}</span>
          </h2>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={CircleDot}
              label="Numri Personal"
              value={rrogat.numriPersonal || "N/A"}
            />
            <InfoItem
              icon={Briefcase}
              label="Profesioni"
              value={rrogat.profesioni || "N/A"}
            />
          </div>
        </div>
      </CardHeader>

      <CardContent className={cardStyles.content}>
        <div className={cardStyles.section}>
          <h3 className={cardStyles.sectionTitle}>
            <Wallet className={cardStyles.sectionIcon} />
            Informacion Financiar
          </h3>
          <div className={cardStyles.detailsContainer}>
            <div className={cardStyles.detailsGrid}>
              <DetailRow label="NIPT" value={rrogat.nipt || "N/A"} />
              <DetailRow label="DRT" value={rrogat.drt || "N/A"} />
              <DetailRow
                label="Paga Bruto"
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
              <DetailRow label="Kategoria" value={rrogat.kategoria || "N/A"} />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

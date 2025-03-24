import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Car, CircleDot } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "./ui/card-styles";
import { TargatResponse } from "@/types/kerko";
import { cn } from "@/lib/utils";

interface TargatCardProps {
  targat: TargatResponse;
  onNameClick: (emri: string, mbiemri: string) => void;
}

export function TargatCard({ targat, onNameClick }: TargatCardProps) {
  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
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
              <div className="col-span-full mt-4">
                <button
                  onClick={() =>
                    onNameClick(targat.emri || "", targat.mbiemri || "")
                  }
                  className={cn(
                    "w-full py-2 px-4 rounded-lg font-medium",
                    "bg-[#1a0808] text-[#999] hover:bg-[#2a1a1a] hover:text-white",
                    "border-2 border-[#2a1a1a]",
                    "transition-all duration-200",
                    "flex items-center justify-center gap-2"
                  )}
                >
                  <CircleDot className="h-4 w-4" />
                  Kërko Pronarin
                </button>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Car, CircleDot, ChevronDown, ChevronUp } from "lucide-react";
import { cardStyles, InfoItem, DetailRow } from "./ui/card-styles";
import { TargatResponse } from "@/types/kerko";
import { cn } from "@/lib/utils";
import { useState } from "react";

interface TargatCardProps {
  targat: TargatResponse;
  onNameClick: (emri: string, mbiemri: string) => void;
}

export function TargatCard({ targat, onNameClick }: TargatCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);

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
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="w-full flex items-center justify-between group hover:opacity-80 transition-opacity"
          >
            <h3 className={cardStyles.sectionTitle}>
              <Car className={cardStyles.sectionIcon} />
              Informacion i Automjetit
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
                      "bg-surface-interactive text-text-tertiary hover:bg-surface-interactive-hover hover:text-text-primary",
                      "border-2 border-border-semantic-secondary",
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
          )}
        </div>
      </CardContent>
    </Card>
  );
}

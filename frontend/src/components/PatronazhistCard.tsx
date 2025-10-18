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
import { useState } from "react";

interface PatronazhistCardProps {
  patronazhist: PatronazhistResponse;
}

export function PatronazhistCard({ patronazhist }: PatronazhistCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  return (
    <Card className={cardStyles.root}>
      <CardHeader className={cardStyles.header}>
        <div className="relative z-10">
          <h2 className={cardStyles.title}>
            {patronazhist.emri}{" "}
            <span className="font-bold">{patronazhist.mbiemri}</span>
          </h2>
          <div className={cardStyles.infoList}>
            <InfoItem
              icon={CircleDot}
              label="Numri Personal"
              value={patronazhist.numriPersonal || "N/A"}
            />
            <InfoItem
              icon={MapPin}
              label="Vendlindja"
              value={patronazhist.vendlindja || "N/A"}
            />
            <InfoItem
              icon={Calendar}
              label="Lindur"
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
                  label="Emri i Babait"
                  value={patronazhist.atesi || "N/A"}
                />
                <DetailRow
                  label="Kod Banese"
                  value={patronazhist.kodBanese || "N/A"}
                />
                <DetailRow label="Telefoni" value={patronazhist.tel || "N/A"} />
                <DetailRow
                  label="Preferenca"
                  value={patronazhist.preferenca || "N/A"}
                />
                <DetailRow
                  label="Preferenca e Sigurte"
                  value={patronazhist.iSigurte || "N/A"}
                />
                <DetailRow
                  label="Preferenca Census 2013"
                  value={patronazhist.census2013Preferenca || "N/A"}
                />
                <DetailRow
                  label="Siguria Census 2013"
                  value={
                    patronazhist.census2013Siguria
                      ? `${patronazhist.census2013Siguria}/10`
                      : "N/A"
                  }
                />
                {patronazhist.koment && (
                  <DetailRow
                    label="Koment"
                    value={patronazhist.koment || "N/A"}
                  />
                )}
                <DetailRow
                  label="Patronazhisti"
                  value={patronazhist.patronazhisti || "N/A"}
                />
                <DetailRow label="QV" value={patronazhist.qv || "N/A"} />
                <DetailRow
                  label="Numri i Listës"
                  value={patronazhist.listaNr || "N/A"}
                />
                <DetailRow
                  label="Kompania"
                  value={patronazhist.kompania || "N/A"}
                />
                {patronazhist.emigrant && (
                  <>
                    <DetailRow
                      label="Emigrant"
                      value={patronazhist.emigrant || "N/A"}
                    />
                    {patronazhist.country && (
                      <DetailRow
                        label="Vendi i Emigrimit"
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

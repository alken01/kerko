import { PatronazhistResponse } from "@/types/search";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Calendar, MapPin, Building2 } from "lucide-react";

interface PatronazhistCardProps {
  patronazhist: PatronazhistResponse;
}

export function PatronazhistCard({ patronazhist }: PatronazhistCardProps) {
  const formatDate = (dateString: string | null) => {
    if (!dateString) return "N/A";
    try {
      // Parse DD/MM/YYYY format
      const [day, month, year] = dateString.split("/");
      const date = new Date(parseInt(year), parseInt(month) - 1, parseInt(day));
      if (isNaN(date.getTime())) return "N/A";
      return new Intl.DateTimeFormat("sq-AL", {
        day: "2-digit",
        month: "short",
        year: "numeric",
      }).format(date);
    } catch {
      return "N/A";
    }
  };

  return (
    <Card className="overflow-hidden border-2 border-[#2a1a1a] bg-[#0a0303] shadow-[0_0_15px_rgba(0,0,0,0.5)]">
      <CardHeader className="bg-gradient-to-br from-[#120606] to-[#0a0303] py-4 px-5 border-b-2 border-[#2a1a1a] relative">
        <div className="absolute inset-0 bg-gradient-to-br from-[#ffffff05] to-transparent opacity-30"></div>
        <div className="relative z-10">
          <h2 className="text-xl font-bold tracking-tight text-white uppercase">
            {patronazhist.emri}{" "}
            <span className="font-bold">{patronazhist.mbiemri}</span>
          </h2>
          <div className="mt-1.5 space-y-1">
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <Calendar className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">Lindur:</span>{" "}
              {formatDate(patronazhist.datelindja)}
            </p>
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <MapPin className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">Vendlindja:</span>{" "}
              <span className="uppercase">
                {patronazhist.vendlindja || "N/A"}
              </span>
            </p>
          </div>
        </div>
      </CardHeader>

      <CardContent className="p-0">
        <div className="px-5 py-3">
          <h3 className="text-base font-bold text-white mb-2 flex items-center gap-2">
            <Building2 className="h-4 w-4 text-[#cccccc]" />
            Informacion Patronazhi
          </h3>
          <div className="bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]">
            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">QV</span>
                <span className="text-white font-bold">
                  {patronazhist.qv || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Numri i Listës</span>
                <span className="text-white font-bold">
                  {patronazhist.listaNr || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Telefoni</span>
                <span className="text-white font-bold">
                  {patronazhist.tel || "N/A"}
                </span>
              </div>
              {patronazhist.emigrant && (
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">
                    Vendi i Emigrimit
                  </span>
                  <span className="text-white font-bold">
                    {patronazhist.country || "N/A"}
                  </span>
                </div>
              )}
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Patronazhisti</span>
                <span className="text-white font-bold uppercase">
                  {patronazhist.patronazhisti || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">Preferenca</span>
                <span className="text-white font-bold uppercase">
                  {patronazhist.preferenca || "N/A"}
                </span>
              </div>
              {patronazhist.koment && (
                <p className="text-sm text-white mt-2 font-normal italic">
                  {patronazhist.koment}
                </p>
              )}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

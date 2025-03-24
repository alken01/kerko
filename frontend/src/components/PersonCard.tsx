import { PersonResponse } from "@/types/search";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Calendar, MapPin, User, Home } from "lucide-react";

interface PersonCardProps {
  person: PersonResponse;
}

export function PersonCard({ person }: PersonCardProps) {
  const formatDate = (dateString: string | null) => {
    if (!dateString) return "N/A";
    try {
      // Parse DD/MM/YY format
      const [month, day, year] = dateString.split("/");
      const date = new Date(
        2000 + parseInt(year),
        parseInt(month) - 1,
        parseInt(day)
      );
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
      <CardHeader className="bg-gradient-to-br from-[#120606] to-[#0a0303] py-4 px-5 border-b border-[#2a1a1a] relative">
        <div className="absolute inset-0 bg-gradient-to-br from-[#ffffff05] to-transparent opacity-30"></div>
        <div className="relative z-10">
          <h2 className="text-xl font-bold tracking-tight text-white uppercase">
            {person.emri} <span className="font-bold">{person.mbiemri}</span>
          </h2>
          <div className="mt-1.5 space-y-1">
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <Calendar className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">Lindur:</span>{" "}
              {formatDate(person.datelindja)}
            </p>
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <MapPin className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">Vendlindja:</span>{" "}
              <span className="uppercase">{person.vendlindja || "N/A"}</span>
            </p>
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <Home className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">Qyteti:</span>{" "}
              <span className="uppercase">{person.qyteti || "N/A"}</span>
            </p>
          </div>
        </div>
      </CardHeader>

      <CardContent className="p-0">
        <div className="px-5 py-3">
          <h3 className="text-base font-bold text-white mb-2 flex items-center gap-2">
            <User className="h-4 w-4 text-[#cccccc]" />
            Informacion Personal
          </h3>
          <div className="bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">Adresa</span>
                <span className="text-white font-bold text-right sm:text-left uppercase">
                  {person.adresa && person.nrBaneses
                    ? `${person.adresa} ${person.nrBaneses}`
                    : person.adresa || person.nrBaneses || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">Emri i Babait</span>
                <span className="text-white font-bold uppercase">
                  {person.atesi || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">Emri i Nënës</span>
                <span className="text-white font-bold uppercase">
                  {person.amesi || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">Kombësia</span>
                <span className="text-white font-bold uppercase">
                  {person.kombesia || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">Gjinia</span>
                <span className="text-white font-bold">
                  {person.seksi === "F" ? "Femër" : "Mashkull"}
                </span>
              </div>
              <div className="flex justify-between items-center sm:flex-col sm:items-start">
                <span className="text-[#999] font-normal">
                  Gjendja Martesore
                </span>
                <span className="text-white font-bold">
                  {person.gjendjeCivile || "N/A"}
                </span>
              </div>
              {person.lidhjaMeKryefamiljarin && (
                <div className="flex justify-between items-center sm:flex-col sm:items-start">
                  <span className="text-[#999] font-normal">
                    Lidhja me Kryefamiljarin
                  </span>
                  <span className="text-white font-bold">
                    {person.lidhjaMeKryefamiljarin}
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

import { RrogatResponse } from "@/types/search";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Wallet, Briefcase } from "lucide-react";

interface RrogatCardProps {
  rrogat: RrogatResponse;
}

export function RrogatCard({ rrogat }: RrogatCardProps) {
  return (
    <Card className="overflow-hidden border-2 border-[#2a1a1a] bg-[#0a0303] shadow-[0_0_15px_rgba(0,0,0,0.5)]">
      <CardHeader className="bg-gradient-to-br from-[#120606] to-[#0a0303] py-4 px-5 border-b border-[#2a1a1a] relative">
        <div className="absolute inset-0 bg-gradient-to-br from-[#ffffff05] to-transparent opacity-30"></div>
        <div className="relative z-10">
          <h2 className="text-xl font-bold tracking-tight text-white uppercase">
            {rrogat.emri} <span className="font-bold">{rrogat.mbiemri}</span>
          </h2>
          <div className="mt-1.5 space-y-1">
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <CircleDot className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">
                Numri Personal:
              </span>{" "}
              <span className="uppercase">{rrogat.numriPersonal || "N/A"}</span>
            </p>
            <p className="text-[#999] flex items-start gap-1.5 text-sm">
              <Briefcase className="h-3.5 w-3.5 text-[#cccccc] mt-0.5" />
              <span className="text-[#666] font-normal">Profesioni:</span>{" "}
              <span className="uppercase">{rrogat.profesioni || "N/A"}</span>
            </p>
          </div>
        </div>
      </CardHeader>

      <CardContent className="p-0">
        <div className="px-5 py-3">
          <h3 className="text-base font-bold text-white mb-2 flex items-center gap-2">
            <Wallet className="h-4 w-4 text-[#cccccc]" />
            Informacion Financiar
          </h3>
          <div className="bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]">
            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">NIPT</span>
                <span className="text-white font-bold">
                  {rrogat.nipt || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">DRT</span>
                <span className="text-white font-bold">
                  {rrogat.drt || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Paga Bruto</span>
                <div className="text-lg font-bold text-white">
                  {rrogat.pagaBruto
                    ? new Intl.NumberFormat("sq-AL").format(rrogat.pagaBruto)
                    : "N/A"}
                  {rrogat.pagaBruto && (
                    <span className="text-[#cccccc] ml-1">ALL</span>
                  )}
                </div>
              </div>
              <div className="flex justify-between items-start">
                <span className="text-[#999] font-normal">Kategoria</span>
                <span className="text-white font-bold text-right pl-4">
                  {rrogat.kategoria || "N/A"}
                </span>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

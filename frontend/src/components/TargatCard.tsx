import { TargatResponse } from "@/types/search";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Car, CircleDot } from "lucide-react";

interface TargatCardProps {
  targat: TargatResponse;
}

export function TargatCard({ targat }: TargatCardProps) {
  return (
    <Card className="overflow-hidden border-2 border-[#2a1a1a] bg-[#0a0303] shadow-[0_0_15px_rgba(0,0,0,0.5)]">
      <CardHeader className="bg-gradient-to-br from-[#120606] to-[#0a0303] py-4 px-5 border-b border-[#2a1a1a] relative">
        <div className="absolute inset-0 bg-gradient-to-br from-[#ffffff05] to-transparent opacity-30"></div>
        <div className="relative z-10">
          <h2 className="text-xl font-bold tracking-tight text-white uppercase">
            {targat.emri} <span className="font-bold">{targat.mbiemri}</span>
          </h2>
          <div className="mt-1.5 space-y-1">
            <p className="text-[#999] flex items-center gap-1.5 text-sm">
              <CircleDot className="h-3.5 w-3.5 text-[#cccccc]" />
              <span className="text-[#666] font-normal">
                Numri Personal:
              </span>{" "}
              <span className="uppercase">{targat.numriPersonal || "N/A"}</span>
            </p>
          </div>
        </div>
      </CardHeader>

      <CardContent className="p-0">
        <div className="px-5 py-3">
          <h3 className="text-base font-bold text-white mb-2 flex items-center gap-2">
            <Car className="h-4 w-4 text-[#cccccc]" />
            Informacion i Automjetit
          </h3>
          <div className="bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]">
            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Marka</span>
                <span className="text-white font-bold">
                  {targat.marka || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Modeli</span>
                <span className="text-white font-bold">
                  {targat.modeli || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Ngjyra</span>
                <span className="text-white font-bold">
                  {targat.ngjyra || "N/A"}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-[#999] font-normal">Numri Personal</span>
                <span className="text-white font-bold">
                  {targat.numriPersonal || "N/A"}
                </span>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

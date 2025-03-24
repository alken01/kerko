"use client";

import { Person } from "@/types/person";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { motion } from "framer-motion";
import { Calendar, MapPin, User, Car, Wallet, Building2 } from "lucide-react";

interface PersonDetailsProps {
  person: Person;
}

export function PersonDetails({ person }: PersonDetailsProps) {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("sq-AL", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    }).format(date);
  };

  const calculateAge = (dateOfBirth: string) => {
    const birthDate = new Date(dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (
      monthDiff < 0 ||
      (monthDiff === 0 && today.getDate() < birthDate.getDate())
    ) {
      age--;
    }

    return age;
  };

  const displayName =
    person.previousSurnames?.length > 0 ? (
      <span>
        {person.firstName} <span className="font-bold">{person.lastName}</span>{" "}
        (
        <span className="font-bold">
          {person.previousSurnames[0].previousSurnameName}
        </span>
        )
      </span>
    ) : (
      <span>
        {person.firstName} <span className="font-bold">{person.lastName}</span>
      </span>
    );

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.3 }}
      className="w-full max-w-md mx-auto"
    >
      <Card className="overflow-hidden border-2 border-[#2a1a1a] bg-[#0a0303] shadow-[0_0_15px_rgba(0,0,0,0.5)]">
        <CardHeader className="bg-gradient-to-br from-[#120606] to-[#0a0303] py-4 px-5 border-b border-[#2a1a1a] relative">
          <div className="absolute inset-0 bg-gradient-to-br from-[#ffffff05] to-transparent opacity-30"></div>
          <div className="relative z-10">
            <h2 className="text-xl font-bold tracking-tight text-white">
              {displayName}
            </h2>
            <div className="mt-1.5 space-y-1">
              <p className="text-[#999] flex items-center gap-1.5 text-sm">
                <Calendar className="h-3.5 w-3.5 text-[#cccccc]" />
                <span className="text-[#666] font-normal">Lindur:</span>{" "}
                {formatDate(person.birthDate)}
                <Badge
                  variant="outline"
                  className="ml-1 bg-[#120606] text-[#cccccc] border-[#2a1a1a] hover:bg-[#2a1a1a] text-xs font-normal"
                >
                  {calculateAge(person.birthDate)} vjet
                </Badge>
              </p>
              <p className="text-[#999] flex items-center gap-1.5 text-sm">
                <MapPin className="h-3.5 w-3.5 text-[#cccccc]" />
                <span className="text-[#666] font-normal">Qyteti:</span>{" "}
                {person.city.cityName}
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
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">
                    Numri Personal
                  </span>
                  <span className="text-white font-bold">
                    {person.personalNumber}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">Emri i Babait</span>
                  <span className="text-white font-bold">
                    {person.fatherName}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">Emri i Nënës</span>
                  <span className="text-white font-bold">
                    {person.motherName}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">Kombësia</span>
                  <span className="text-white font-bold">
                    {person.nationality.nationalityName}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">Gjinia</span>
                  <span className="text-white font-bold">
                    {person.gender === "F" ? "Femër" : "Mashkull"}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">
                    Gjendja Martesore
                  </span>
                  <span className="text-white font-bold">
                    {person.maritalStatus.status}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-[#999] font-normal">Adresa</span>
                  <span className="text-white font-bold">{`${person.address} ${person.houseNumber}`}</span>
                </div>
              </div>
            </div>
          </div>

          {person.patronazhInfo && (
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
                      {person.patronazhInfo.qv}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">
                      Numri i Listës
                    </span>
                    <span className="text-white font-bold">
                      {person.patronazhInfo.listNumber}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Telefoni</span>
                    <span className="text-white font-bold">
                      {person.patronazhInfo.phone}
                    </span>
                  </div>
                  {person.patronazhInfo.isEmigrant && (
                    <div className="flex justify-between items-center">
                      <span className="text-[#999] font-normal">
                        Vendi i Emigrimit
                      </span>
                      <span className="text-white font-bold">
                        {person.patronazhInfo.emigrantCountry}
                      </span>
                    </div>
                  )}
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">
                      Patronazhisti
                    </span>
                    <span className="text-white font-bold">
                      {person.patronazhInfo.patronazhist}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Preferenca</span>
                    <span className="text-white font-bold">
                      {person.patronazhInfo.preference}
                    </span>
                  </div>
                  {person.patronazhInfo.comment && (
                    <p className="text-sm text-white mt-2 font-normal italic">
                      {person.patronazhInfo.comment}
                    </p>
                  )}
                </div>
              </div>
            </div>
          )}

          {person.vehicle && (
            <div className="px-5 py-3">
              <h3 className="text-base font-bold text-white mb-2 flex items-center gap-2">
                <Car className="h-4 w-4 text-[#cccccc]" />
                Informacion i Automjetit
              </h3>
              <div className="bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]">
                <div className="space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Targa</span>
                    <Badge className="bg-[#120606] hover:bg-[#2a1a1a] text-[#cccccc] border-[#2a1a1a] font-mono">
                      {person.vehicle.licensePlate}
                    </Badge>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Marka</span>
                    <span className="text-white font-bold">
                      {person.vehicle.brand}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Modeli</span>
                    <span className="text-white font-bold">
                      {person.vehicle.model}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Ngjyra</span>
                    <span className="text-white font-bold">
                      {person.vehicle.color}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          )}

          {person.salary && (
            <div className="px-5 py-3">
              <h3 className="text-base font-bold text-white mb-2 flex items-center gap-2">
                <Wallet className="h-4 w-4 text-[#cccccc]" />
                Informacion Financiar
              </h3>
              <div className="bg-[#120606] rounded-lg p-3 shadow-[inset_0_1px_1px_rgba(0,0,0,0.2),0_1px_1px_rgba(255,255,255,0.05)] border-2 border-[#2a1a1a]">
                <div className="space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Kompania</span>
                    <span className="text-white font-bold">
                      {person.salary.company}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Pozita</span>
                    <span className="text-white font-bold">
                      {person.salary.position}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Paga Bruto</span>
                    <div className="text-lg font-bold text-white">
                      {new Intl.NumberFormat("sq-AL").format(
                        person.salary.grossSalary
                      )}
                      <span className="text-[#cccccc] ml-1">ALL</span>
                    </div>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">Kategoria</span>
                    <span className="text-white font-bold">
                      {person.salary.category}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-[#999] font-normal">
                      Lloji i Punësimit
                    </span>
                    <span className="text-white font-bold">
                      {person.salary.employmentType}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
}

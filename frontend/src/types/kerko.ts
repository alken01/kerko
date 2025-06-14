export interface PersonResponse {
  adresa: string | null;
  nrBaneses: string | null;
  emri: string | null;
  mbiemri: string | null;
  atesi: string | null;
  amesi: string | null;
  datelindja: string | null;
  vendlindja: string | null;
  seksi: string | null;
  lidhjaMeKryefamiljarin: string | null;
  qyteti: string | null;
  gjendjeCivile: string | null;
  kombesia: string | null;
}

export interface RrogatResponse {
  numriPersonal: string | null;
  emri: string | null;
  mbiemri: string | null;
  nipt: string | null;
  drt: string | null;
  pagaBruto: number | null;
  profesioni: string | null;
  kategoria: string | null;
}

export interface TargatResponse {
  numriTarges: string | null;
  marka: string | null;
  modeli: string | null;
  ngjyra: string | null;
  numriPersonal: string | null;
  emri: string | null;
  mbiemri: string | null;
}

export interface PatronazhistResponse {
  numriPersonal: string | null;
  emri: string | null;
  mbiemri: string | null;
  atesi: string | null;
  datelindja: string | null;
  qv: string | null;
  listaNr: string | null;
  tel: string | null;
  emigrant: string | null;
  country: string | null;
  iSigurte: string | null;
  koment: string | null;
  patronazhisti: string | null;
  preferenca: string | null;
  census2013Preferenca: string | null;
  census2013Siguria: string | null;
  vendlindja: string | null;
  kompania: string | null;
  kodBanese: string | null;
}

export interface SearchResponse {
  person: PersonResponse[];
  rrogat: RrogatResponse[];
  targat: TargatResponse[];
  patronazhist: PatronazhistResponse[];
}

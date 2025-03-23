export interface Person {
  id: number;
  personalNumber: string;
  firstName: string;
  lastName: string;
  fatherName: string;
  motherName: string;
  birthDate: string;
  birthPlace: string;
  gender: string;
  address: string;
  houseNumber: string;
  cityId: number;
  maritalStatusId: number;
  nationalityId: number;
  relationshipId: number;
  city: City;
  maritalStatus: MaritalStatus;
  nationality: Nationality;
  relationship: Relationship;
  previousSurnames: PreviousSurname[];
  patronazhInfo: PatronazhInfo;
  vehicle: Vehicle;
  salary: Salary;
}

export interface Nationality {
  id: number;
  nationalityName: string;
}

export interface MaritalStatus {
  id: number;
  status: string;
}

export interface City {
  id: number;
  cityName: string;
}

export interface PreviousSurname {
  id: number;
  personId: number;
  previousSurnameName: string;
}

export interface PatronazhInfo {
  id: number;
  personId: number;
  qv: string;
  listNumber: string;
  phone: string;
  isEmigrant: boolean;
  emigrantCountry: string | null;
  isCertain: boolean;
  comment: string;
  patronazhist: string;
  preference: string;
  censusPreference: string;
  certainty: string;
  birthplace: string;
  company: string;
  houseCode: string;
}

export interface Vehicle {
  id: number;
  personId: number;
  licensePlate: string;
  brand: string;
  model: string;
  color: string;
}

export interface Salary {
  id: number;
  personId: number;
  personalNumber: string;
  nipt: string;
  company: string;
  taxOffice: string;
  grossSalary: number;
  position: string;
  category: string;
  employmentType: string;
}

export interface Relationship {
  id: number;
  relationshipName: string;
}

import { NextRequest, NextResponse } from "next/server";

// Mock data based on the C# models
const mockPerson = {
  id: 24,
  personalNumber: "A123456",
  firstName: "SONILA",
  lastName: "UZNOVA",
  fatherName: "ALEXANDER",
  motherName: "MARIA",
  birthDate: "1979-09-18",
  birthPlace: "TIRANA",
  gender: "F",
  address: "Rruga e Durrësit 123",
  houseNumber: "45",

  // Related entities
  city: {
    id: 1,
    cityName: "TIRANA",
  },
  maritalStatus: {
    id: 1,
    status: "SINGLE",
  },
  nationality: {
    id: 1,
    nationalityName: "ALBANIAN",
  },
  relationship: {
    id: 1,
    relationshipName: "HEAD_OF_HOUSEHOLD",
  },

  // Collections
  previousSurnames: [
    {
      id: 1,
      personId: 24,
      previousSurnameName: "BARDHYL",
    },
  ],

  patronazhInfo: {
    id: 1,
    personId: 24,
    qv: "QV123",
    listNumber: "L456",
    phone: "+355 69 123 4567",
    isEmigrant: false,
    emigrantCountry: null,
    isCertain: true,
    comment: "Regular check-ups required",
    patronazhist: "DR. SMITH",
    preference: "MORNING",
    censusPreference: "HOME",
    certainty: "HIGH",
    birthplace: "TIRANA",
    company: "HEALTHCARE CENTER",
    houseCode: "HC789",
  },

  vehicle: {
    id: 1,
    personId: 24,
    licensePlate: "AB123CD",
    brand: "TOYOTA",
    model: "COROLLA",
    color: "SILVER",
  },

  salary: {
    id: 1,
    personId: 24,
    personalNumber: "A123456",
    nipt: "N12345678A",
    company: "TECH COMPANY",
    taxOffice: "TIRANA TAX OFFICE",
    grossSalary: 85000,
    position: "SENIOR DEVELOPER",
    category: "IT",
    employmentType: "FULL_TIME",
  },
};

export async function GET(request: NextRequest) {
  const searchParams = request.nextUrl.searchParams;
  const firstName = searchParams.get("firstName");
  const lastName = searchParams.get("lastName");

  if (!firstName || !lastName) {
    return NextResponse.json(
      { error: "First name and last name are required" },
      { status: 400 }
    );
  }

  // Simulate API delay
  await new Promise((resolve) => setTimeout(resolve, 500));

  // Simple mock search - return data if first name or last name matches
  if (
    firstName.toUpperCase().includes("SONILA") ||
    lastName.toUpperCase().includes("UZNOVA")
  ) {
    return NextResponse.json(mockPerson);
  }

  // Return 404 if no match found
  return NextResponse.json({ error: "Person not found" }, { status: 404 });
}

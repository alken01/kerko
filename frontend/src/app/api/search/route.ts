import { NextRequest, NextResponse } from "next/server";
import { SearchResponse } from "@/types/kerko";

export async function GET(request: NextRequest) {
  const searchParams = request.nextUrl.searchParams;
  const firstName = searchParams.get("firstName");
  const lastName = searchParams.get("lastName");

  if (!firstName || !lastName) {
    return NextResponse.json(
      { error: "Emri dhe mbiemri janë të detyrueshëm" },
      { status: 400 }
    );
  }

  try {
    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    const response = await fetch(
      `${API_URL}/api/kerko?emri=${encodeURIComponent(
        firstName
      )}&mbiemri=${encodeURIComponent(lastName)}`,
      {
        headers: {
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      if (response.status === 404) {
        return NextResponse.json(
          { error: "Nuk u gjet asnjë rezultat" },
          { status: 404 }
        );
      }
      throw new Error("Failed to fetch from backend API");
    }

    const data = await response.json();
    return NextResponse.json(data as SearchResponse);
  } catch (error) {
    console.error("API Error:", error);
    return NextResponse.json(
      { error: "Pati një problem gjatë kërkimit" },
      { status: 500 }
    );
  }
}

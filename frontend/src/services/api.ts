import { SearchResponse } from "@/types/kerko";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export class ApiService {
  static async searchPerson(
    emri: string,
    mbiemri: string
  ): Promise<SearchResponse> {
    const response = await fetch(
      `${API_URL}/api/kerko?emri=${encodeURIComponent(
        emri
      )}&mbiemri=${encodeURIComponent(mbiemri)}`,
      {
        headers: {
          'ngrok-skip-browser-warning': 'true'
        }
      }
    );

    if (!response.ok) {
      if (response.status === 429) {
        throw new Error("Qetsohu cik mplak, prit pak edhe provo prap");
      }
      const text = await response.text();
      throw new Error(text || "Pati një problem gjatë kërkimit të personit");
    }

    const data: SearchResponse = await response.json();
    if (
      !data ||
      Object.values(data).every(
        (arr) =>
          !Array.isArray(arr) ||
          arr.length === 0 ||
          arr.every((item) => !Object.values(item).some(Boolean))
      )
    ) {
      throw new Error("Nuk u gjet asnjë rezultat");
    }

    return data;
  }

  static async searchTarga(numriTarges: string): Promise<SearchResponse> {
    const response = await fetch(
      `${API_URL}/api/targat?numriTarges=${encodeURIComponent(numriTarges)}`,
      {
        headers: {
          'ngrok-skip-browser-warning': 'true'
        }
      }
    );

    if (!response.ok) {
      if (response.status === 429) {
        throw new Error("Qetsohu cik mplak, prit pak edhe provo prap");
      }
      const text = await response.text();
      throw new Error(text || "Pati një problem gjatë kërkimit të targës");
    }

    const data: SearchResponse = await response.json();

    if (!data || data.targat.length === 0) {
      throw new Error("Nuk u gjet asnjë rezultat");
    }

    return data;
  }
}

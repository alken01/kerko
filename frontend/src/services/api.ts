import { SearchResponse, TargatSearchResponse, PatronazhistSearchResponse } from "@/types/kerko";
import {
  RATE_LIMIT_ERROR_MESSAGE,
  NO_RESULTS_MESSAGE,
  PERSON_SEARCH_ERROR,
  PLATE_SEARCH_ERROR,
  PHONE_SEARCH_ERROR,
  NGROK_HEADER,
  DEFAULT_PAGE_SIZE,
} from "@/lib/constants";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export class ApiService {
  static async searchPerson(
    emri: string,
    mbiemri: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<SearchResponse> {
    const params = new URLSearchParams({
      emri,
      mbiemri,
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    });

    const response = await fetch(
      `${API_URL}/api/kerko?${params}`,
      { headers: NGROK_HEADER }
    );

    if (!response.ok) {
      if (response.status === 429) {
        throw new Error(RATE_LIMIT_ERROR_MESSAGE);
      }
      const text = await response.text();
      throw new Error(text || PERSON_SEARCH_ERROR);
    }

    const data: SearchResponse = await response.json();
    if (
      !data ||
      Object.values(data).every(
        (result) =>
          !result.items ||
          result.items.length === 0 ||
          result.items.every((item: Record<string, unknown>) => !Object.values(item).some(Boolean))
      )
    ) {
      throw new Error(NO_RESULTS_MESSAGE);
    }

    return data;
  }

  static async searchTarga(
    numriTarges: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<TargatSearchResponse> {
    const params = new URLSearchParams({
      numriTarges,
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    });

    const response = await fetch(
      `${API_URL}/api/targat?${params}`,
      { headers: NGROK_HEADER }
    );

    if (!response.ok) {
      if (response.status === 429) {
        throw new Error(RATE_LIMIT_ERROR_MESSAGE);
      }
      const text = await response.text();
      throw new Error(text || PLATE_SEARCH_ERROR);
    }

    const data: TargatSearchResponse = await response.json();

    if (!data || data.items.length === 0) {
      throw new Error(NO_RESULTS_MESSAGE);
    }

    return data;
  }

  static async searchTelefon(
    numriTelefonit: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<PatronazhistSearchResponse> {
    const params = new URLSearchParams({
      numriTelefonit,
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    });

    const response = await fetch(
      `${API_URL}/api/telefon?${params}`,
      { headers: NGROK_HEADER }
    );

    if (!response.ok) {
      if (response.status === 429) {
        throw new Error(RATE_LIMIT_ERROR_MESSAGE);
      }
      const text = await response.text();
      throw new Error(text || PHONE_SEARCH_ERROR);
    }

    const data: PatronazhistSearchResponse = await response.json();

    if (!data || data.items.length === 0) {
      throw new Error(NO_RESULTS_MESSAGE);
    }

    return data;
  }
}

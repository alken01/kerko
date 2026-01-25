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

async function fetchApi<T>(url: string, fallbackError: string): Promise<T> {
  const response = await fetch(url, { headers: NGROK_HEADER });

  if (!response.ok) {
    if (response.status === 429) {
      throw new Error(RATE_LIMIT_ERROR_MESSAGE);
    }
    const text = await response.text();
    throw new Error(text || fallbackError);
  }

  return response.json();
}

function buildParams(params: Record<string, string | number>): URLSearchParams {
  return new URLSearchParams(
    Object.entries(params).reduce((acc, [key, value]) => {
      acc[key] = String(value);
      return acc;
    }, {} as Record<string, string>)
  );
}

export class ApiService {
  static async searchPerson(
    emri: string,
    mbiemri: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<SearchResponse> {
    const params = buildParams({ emri, mbiemri, pageNumber, pageSize });
    const data = await fetchApi<SearchResponse>(
      `${API_URL}/api/kerko?${params}`,
      PERSON_SEARCH_ERROR
    );

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
    const params = buildParams({ numriTarges, pageNumber, pageSize });
    const data = await fetchApi<TargatSearchResponse>(
      `${API_URL}/api/targat?${params}`,
      PLATE_SEARCH_ERROR
    );

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
    const params = buildParams({ numriTelefonit, pageNumber, pageSize });
    const data = await fetchApi<PatronazhistSearchResponse>(
      `${API_URL}/api/telefon?${params}`,
      PHONE_SEARCH_ERROR
    );

    if (!data || data.items.length === 0) {
      throw new Error(NO_RESULTS_MESSAGE);
    }

    return data;
  }
}

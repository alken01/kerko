import { SearchResponse, TargatSearchResponse, PatronazhistSearchResponse, NumriPersonalSearchResponse, RrogatResponse, PaginatedResult } from "@/types/kerko";
import {
  RATE_LIMIT_ERROR_KEY,
  NO_RESULTS_KEY,
  PERSON_SEARCH_ERROR_KEY,
  PLATE_SEARCH_ERROR_KEY,
  PHONE_SEARCH_ERROR_KEY,
  NGROK_HEADER,
  DEFAULT_PAGE_SIZE,
} from "@/lib/constants";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

let currentController: AbortController | null = null;

async function fetchApi<T>(url: string, fallbackErrorKey: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(url, { headers: NGROK_HEADER, signal });

  if (!response.ok) {
    if (response.status === 429) {
      throw new Error(RATE_LIMIT_ERROR_KEY);
    }
    throw new Error(fallbackErrorKey);
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

function getSignal(): AbortSignal {
  if (currentController) {
    currentController.abort();
  }
  currentController = new AbortController();
  return currentController.signal;
}

export class ApiService {
  static async searchPerson(
    emri: string,
    mbiemri: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<SearchResponse> {
    const signal = getSignal();
    const params = buildParams({ emri, mbiemri, pageNumber, pageSize });
    const data = await fetchApi<SearchResponse>(
      `${API_URL}/api/kerko?${params}`,
      PERSON_SEARCH_ERROR_KEY,
      signal
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
      throw new Error(NO_RESULTS_KEY);
    }

    return data;
  }

  static async searchTarga(
    numriTarges: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<TargatSearchResponse> {
    const signal = getSignal();
    const params = buildParams({ numriTarges, pageNumber, pageSize });
    const data = await fetchApi<TargatSearchResponse>(
      `${API_URL}/api/targat?${params}`,
      PLATE_SEARCH_ERROR_KEY,
      signal
    );

    if (!data || data.items.length === 0) {
      throw new Error(NO_RESULTS_KEY);
    }

    return data;
  }

  static async searchTelefon(
    numriTelefonit: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<PatronazhistSearchResponse> {
    const signal = getSignal();
    const params = buildParams({ numriTelefonit, pageNumber, pageSize });
    const data = await fetchApi<PatronazhistSearchResponse>(
      `${API_URL}/api/telefon?${params}`,
      PHONE_SEARCH_ERROR_KEY,
      signal
    );

    if (!data || data.items.length === 0) {
      throw new Error(NO_RESULTS_KEY);
    }

    return data;
  }

  static async searchNumriPersonal(
    numriPersonal: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<NumriPersonalSearchResponse> {
    const signal = getSignal();
    const params = buildParams({ numriPersonal, pageNumber, pageSize });
    const data = await fetchApi<NumriPersonalSearchResponse>(
      `${API_URL}/api/numripersonal?${params}`,
      PERSON_SEARCH_ERROR_KEY,
      signal
    );

    if (
      !data ||
      (data.rrogat.items.length === 0 && data.targat.items.length === 0 && data.patronazhist.items.length === 0)
    ) {
      throw new Error(NO_RESULTS_KEY);
    }

    return data;
  }

  static async searchNipt(
    nipt: string,
    pageNumber: number = 1,
    pageSize: number = DEFAULT_PAGE_SIZE
  ): Promise<PaginatedResult<RrogatResponse>> {
    const signal = getSignal();
    const params = buildParams({ nipt, pageNumber, pageSize });
    const data = await fetchApi<PaginatedResult<RrogatResponse>>(
      `${API_URL}/api/nipt?${params}`,
      PERSON_SEARCH_ERROR_KEY,
      signal
    );

    if (!data || data.items.length === 0) {
      throw new Error(NO_RESULTS_KEY);
    }

    return data;
  }

}

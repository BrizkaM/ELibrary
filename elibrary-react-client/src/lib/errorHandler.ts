import { AxiosError } from "axios";
import type { TranslationKeys } from "./i18n/translations";

interface ApiErrorResponse {
  error?: string;
  message?: string;
  errorCode?: string;
}

export function getErrorMessage(error: unknown, t: TranslationKeys): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as ApiErrorResponse | undefined;

    // Specifické chyby podle errorCode z backendu
    if (data?.errorCode) {
      switch (data.errorCode) {
        case "DUPLICATE_ISBN":
          return t.errors.duplicateIsbn;
        case "NOT_FOUND":
          return t.errors.notFound;
        case "OUT_OF_STOCK":
          return t.errors.outOfStock;
        case "VALIDATION_ERROR":
          return data.message || t.errors.validation;
        case "CONCURRENCY_CONFLICT":
          return t.errors.concurrencyConflict;
        default:
          return data.message || t.errors.unknown;
      }
    }

    // HTTP status chyby
    if (error.response?.status) {
      switch (error.response.status) {
        case 400:
          return data?.message || t.errors.badRequest;
        case 404:
          return t.errors.notFound;
        case 409:
          return data?.message || t.errors.conflict;
        case 500:
          return t.errors.serverError;
        default:
          return data?.message || t.errors.unknown;
      }
    }

    // Network error
    if (error.code === "ERR_NETWORK") {
      return t.errors.networkError;
    }
  }

  return t.errors.unknown;
}
